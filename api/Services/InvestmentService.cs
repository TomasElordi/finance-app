using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class InvestmentService(AppDbContext db) : IInvestmentService
{
    // Keys of the palette the frontend knows how to render.
    public static readonly HashSet<string> Colors =
        ["amber", "slate", "green", "red", "blue", "purple", "teal", "pink"];

    private const int MaxGroups = 50;
    private const int MaxHoldings = 200;

    public async Task<InvestmentPortfolioDto> GetPortfolioAsync(Guid userId)
    {
        var portfolio = await db.InvestmentPortfolios.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);
        var groups = await db.InvestmentGroups.AsNoTracking()
            .Where(g => g.UserId == userId)
            .Include(g => g.Holdings)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
        var income = await db.InvestmentIncomes.AsNoTracking()
            .Where(i => i.UserId == userId)
            .GroupBy(i => i.HoldingId)
            .Select(g => new { HoldingId = g.Key, Total = g.Sum(i => i.AmountUsd) })
            .ToDictionaryAsync(x => x.HoldingId, x => x.Total);
        return BuildPortfolio(portfolio, groups, income);
    }

    // Buy-only rebalance: each holding's target is (current value + contribution) × its share
    // of the target percents; the contribution covers the gaps, proportionally when it can't
    // cover all of them. Units are whole unless the group allows fractions (2 decimals), and
    // what rounding leaves over is spent one whole unit at a time on whatever is furthest
    // below its target and still affordable. Nothing is saved.
    public async Task<ContributionPlanDto> SimulateContributionAsync(Guid userId, SimulateContributionRequestDto request)
    {
        var currency = request.Currency?.Trim().ToUpperInvariant();
        if (currency is not ("USD" or "ARS"))
            throw new ValidationException("Currency must be USD or ARS.");
        if (request.Amount <= 0 || request.Amount > 1_000_000_000_000m)
            throw new ValidationException("The amount must be greater than zero.");

        var portfolio = await GetPortfolioAsync(userId);
        var rate = portfolio.UsdRate;
        if (rate <= 0)
            throw new ValidationException("Set the dollar rate first.");
        return PlanContribution(portfolio, request.Amount, currency);
    }

    // Pure so it can be tested without a database: everything it needs is in the portfolio.
    // ARS amounts are converted at the portfolio's dollar rate.
    internal static ContributionPlanDto PlanContribution(InvestmentPortfolioDto portfolio, decimal amount, string currency)
    {
        var rate = portfolio.UsdRate;
        var amountUsd = currency == "ARS" ? amount / rate : amount;
        var eligible = portfolio.Groups
            .SelectMany(g => g.Holdings.Select(h => (Holding: h, Fractional: g.AllowFractional)))
            .Where(x => x.Holding.TargetPercent > 0 && x.Holding.PriceUsd > 0)
            .ToList();
        var percentSum = eligible.Sum(x => x.Holding.TargetPercent);
        var lines = new List<ContributionLineDto>();
        if (percentSum <= 0)
            return new ContributionPlanDto { AmountUsd = amountUsd, UsdRate = rate, InvestedUsd = 0, LeftoverUsd = amountUsd, Lines = lines };

        var newTotal = portfolio.ValueUsd + amountUsd;
        var gaps = eligible
            .Select(x => Math.Max(0, newTotal * x.Holding.TargetPercent / percentSum - x.Holding.ValueUsd))
            .ToArray();
        var gapSum = gaps.Sum();
        var scale = gapSum > amountUsd ? amountUsd / gapSum : 1;

        var units = eligible
            .Select((x, i) => Math.Round(gaps[i] * scale / x.Holding.PriceUsd, x.Fractional ? 2 : 0, MidpointRounding.ToZero))
            .ToArray();
        var leftover = amountUsd - eligible.Select((x, i) => units[i] * x.Holding.PriceUsd).Sum();

        for (var guard = 0; guard < 1000; guard++)
        {
            var best = -1;
            var bestGap = 0m;
            for (var i = 0; i < eligible.Count; i++)
            {
                var (h, fractional) = eligible[i];
                var remaining = gaps[i] - units[i] * h.PriceUsd;
                if (!fractional && h.PriceUsd <= leftover && remaining > bestGap)
                {
                    best = i;
                    bestGap = remaining;
                }
            }
            if (best < 0)
                break;
            units[best] += 1;
            leftover -= eligible[best].Holding.PriceUsd;
        }

        var invested = amountUsd - leftover;
        var totalAfter = portfolio.ValueUsd + invested;
        for (var i = 0; i < eligible.Count; i++)
        {
            if (units[i] <= 0)
                continue;
            var h = eligible[i].Holding;
            var spent = units[i] * h.PriceUsd;
            lines.Add(new ContributionLineDto
            {
                HoldingId = h.Id,
                Ticker = h.Ticker,
                Units = units[i],
                AmountUsd = spent,
                AmountArs = spent * rate,
                TargetPercent = h.TargetPercent,
                PercentAfter = totalAfter > 0 ? (h.ValueUsd + spent) / totalAfter * 100 : 0,
            });
        }

        return new ContributionPlanDto
        {
            AmountUsd = amountUsd,
            UsdRate = rate,
            InvestedUsd = invested,
            LeftoverUsd = Math.Max(leftover, 0),
            Lines = lines.OrderByDescending(l => l.AmountUsd).ToList(),
        };
    }

    public async Task<InvestmentPortfolioDto> UpdatePortfolioAsync(Guid userId, UpdatePortfolioRequestDto request)
    {
        if (request.TargetAmountUsd < 0)
            throw new ValidationException("The amount to invest can't be negative.");
        ValidateRate(request.UsdRate);

        var portfolio = await GetOrCreatePortfolioAsync(userId);
        portfolio.TargetAmountUsd = request.TargetAmountUsd;
        portfolio.UsdRate = request.UsdRate;
        portfolio.UpdatedAt = DateTime.UtcNow;
        return await SaveAndSnapshotAsync(userId);
    }

    public async Task<InvestmentPortfolioDto> UpdatePricesAsync(Guid userId, UpdateInvestmentPricesRequestDto request)
    {
        ValidateRate(request.UsdRate);
        if (request.Prices.Count > MaxHoldings)
            throw new ValidationException($"Too many prices (max {MaxHoldings}).");
        if (request.Prices.Any(p => p.PriceArs < 0))
            throw new ValidationException("Prices can't be negative.");
        if (request.Prices.Select(p => p.HoldingId).Distinct().Count() != request.Prices.Count)
            throw new ValidationException("Each holding can appear only once.");

        var ids = request.Prices.Select(p => p.HoldingId).ToList();
        var holdings = await db.InvestmentHoldings
            .Where(h => h.UserId == userId && ids.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id);
        if (holdings.Count != ids.Count)
            throw new NotFoundException("One or more holdings were not found.");

        var now = DateTime.UtcNow;
        foreach (var price in request.Prices)
        {
            var holding = holdings[price.HoldingId];
            holding.PriceArs = price.PriceArs;
            holding.PriceUpdatedAt = now;
        }

        var portfolio = await GetOrCreatePortfolioAsync(userId);
        portfolio.UsdRate = request.UsdRate;
        portfolio.UpdatedAt = now;
        return await SaveAndSnapshotAsync(userId);
    }

    public async Task<InvestmentGroupDto> CreateGroupAsync(Guid userId, InvestmentGroupRequestDto request)
    {
        var (name, color) = ValidateGroup(request);
        var existing = await db.InvestmentGroups.Where(g => g.UserId == userId).ToListAsync();
        if (existing.Count >= MaxGroups)
            throw new ValidationException($"Too many groups (max {MaxGroups}).");
        if (existing.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            throw new ConflictException("A group with that name already exists.");

        var group = new InvestmentGroup
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Color = color,
            SortOrder = existing.Count == 0 ? 0 : existing.Max(g => g.SortOrder) + 1,
            AllowFractional = request.AllowFractional,
        };
        db.InvestmentGroups.Add(group);
        var portfolio = await SaveAndSnapshotAsync(userId);
        return portfolio.Groups.First(g => g.Id == group.Id);
    }

    public async Task<InvestmentGroupDto> UpdateGroupAsync(Guid userId, Guid groupId, InvestmentGroupRequestDto request)
    {
        var (name, color) = ValidateGroup(request);
        var group = await db.InvestmentGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId)
            ?? throw new NotFoundException("Group not found.");
        var nameTaken = await db.InvestmentGroups
            .AnyAsync(g => g.UserId == userId && g.Id != groupId && g.Name.ToLower() == name.ToLower());
        if (nameTaken)
            throw new ConflictException("A group with that name already exists.");

        group.Name = name;
        group.Color = color;
        group.AllowFractional = request.AllowFractional;
        var portfolio = await SaveAndSnapshotAsync(userId);
        return portfolio.Groups.First(g => g.Id == group.Id);
    }

    public async Task DeleteGroupAsync(Guid userId, Guid groupId)
    {
        var group = await db.InvestmentGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId)
            ?? throw new NotFoundException("Group not found.");
        if (await db.InvestmentHoldings.AnyAsync(h => h.GroupId == groupId))
            throw new ConflictException("The group still has holdings. Delete or move them first.");
        db.InvestmentGroups.Remove(group);
        await SaveAndSnapshotAsync(userId);
    }

    public async Task<InvestmentHoldingDto> CreateHoldingAsync(Guid userId, InvestmentHoldingRequestDto request)
    {
        var ticker = await ValidateHoldingAsync(userId, request, null);
        if (await db.InvestmentHoldings.CountAsync(h => h.UserId == userId) >= MaxHoldings)
            throw new ValidationException($"Too many holdings (max {MaxHoldings}).");

        var holding = new InvestmentHolding
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GroupId = request.GroupId,
            Ticker = ticker,
            MarketSymbol = NormalizeSymbol(request.MarketSymbol),
            AutoPrice = request.AutoPrice,
            TargetPercent = request.TargetPercent,
            PriceArs = request.PriceArs,
            PriceUpdatedAt = DateTime.UtcNow,
            Quantity = 0,
            AverageCostUsd = 0,
        };
        if (request.InitialTrade is { } first)
        {
            if (first.Type != InvestmentTradeType.Buy)
                throw new ValidationException("The first trade of a new holding must be a buy.");
            holding.Trades.Add(NewTrade(userId, holding.Id, first));
            Recalculate(holding);
        }
        db.InvestmentHoldings.Add(holding);
        return HoldingOf(await SaveAndSnapshotAsync(userId), holding.Id);
    }

    public async Task<InvestmentHoldingDto> UpdateHoldingAsync(Guid userId, Guid holdingId, InvestmentHoldingRequestDto request)
    {
        var holding = await db.InvestmentHoldings.FirstOrDefaultAsync(h => h.Id == holdingId && h.UserId == userId)
            ?? throw new NotFoundException("Holding not found.");
        var ticker = await ValidateHoldingAsync(userId, request, holdingId);

        if (holding.PriceArs != request.PriceArs)
            holding.PriceUpdatedAt = DateTime.UtcNow;
        holding.GroupId = request.GroupId;
        holding.Ticker = ticker;
        holding.MarketSymbol = NormalizeSymbol(request.MarketSymbol);
        holding.AutoPrice = request.AutoPrice;
        holding.TargetPercent = request.TargetPercent;
        holding.PriceArs = request.PriceArs;
        return HoldingOf(await SaveAndSnapshotAsync(userId), holding.Id);
    }

    public async Task<InvestmentActivityDto> GetActivityAsync(Guid userId, Guid? holdingId)
    {
        var trades = await db.InvestmentTrades.AsNoTracking()
            .Where(t => t.UserId == userId && (holdingId == null || t.HoldingId == holdingId))
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt)
            .Take(1000)
            .Select(t => new InvestmentTradeDto
            {
                Id = t.Id,
                HoldingId = t.HoldingId,
                Ticker = t.Holding.Ticker,
                Date = t.Date,
                Type = t.Type,
                Quantity = t.Quantity,
                PriceArs = t.PriceArs,
                UsdRate = t.UsdRate,
                PriceUsd = t.PriceUsd,
                FeeArs = t.FeeArs,
                FeeUsd = t.FeeUsd,
                IsInitial = t.IsInitial,
            })
            .ToListAsync();
        var incomes = await db.InvestmentIncomes.AsNoTracking()
            .Where(i => i.UserId == userId && (holdingId == null || i.HoldingId == holdingId))
            .OrderByDescending(i => i.Date).ThenByDescending(i => i.CreatedAt)
            .Take(1000)
            .Select(i => new InvestmentIncomeDto
            {
                Id = i.Id,
                HoldingId = i.HoldingId,
                Ticker = i.Holding.Ticker,
                Date = i.Date,
                Type = i.Type,
                AmountUsd = i.AmountUsd,
                AmountArs = i.AmountArs,
                UsdRate = i.UsdRate,
            })
            .ToListAsync();
        return new InvestmentActivityDto { Trades = trades, Incomes = incomes };
    }

    private const int MaxTradesPerHolding = 2000;

    public async Task<InvestmentHoldingDto> AddTradeAsync(Guid userId, InvestmentTradeRequestDto request)
    {
        var holding = await LoadHoldingWithTradesAsync(userId, request.HoldingId);
        if (holding.Trades.Count >= MaxTradesPerHolding)
            throw new ValidationException($"Too many trades for {holding.Ticker} (max {MaxTradesPerHolding}).");
        var trade = NewTrade(userId, holding.Id, request);
        holding.Trades.Add(trade);
        Recalculate(holding);
        db.InvestmentTrades.Add(trade);
        return HoldingOf(await SaveAndSnapshotAsync(userId), holding.Id);
    }

    public async Task<InvestmentHoldingDto> DeleteTradeAsync(Guid userId, Guid tradeId)
    {
        var trade = await db.InvestmentTrades.FirstOrDefaultAsync(t => t.Id == tradeId && t.UserId == userId)
            ?? throw new NotFoundException("Trade not found.");
        var holding = await LoadHoldingWithTradesAsync(userId, trade.HoldingId);
        holding.Trades.RemoveAll(t => t.Id == tradeId);
        Recalculate(holding);
        db.InvestmentTrades.Remove(trade);
        return HoldingOf(await SaveAndSnapshotAsync(userId), holding.Id);
    }

    public async Task<InvestmentHoldingDto> AddIncomeAsync(Guid userId, InvestmentIncomeRequestDto request)
    {
        var holding = await db.InvestmentHoldings.FirstOrDefaultAsync(h => h.Id == request.HoldingId && h.UserId == userId)
            ?? throw new NotFoundException("Holding not found.");
        var date = ValidateDate(request.Date);
        if (request.Amount <= 0)
            throw new ValidationException("The amount must be greater than zero.");
        var currency = request.Currency?.Trim().ToUpperInvariant();
        decimal amountUsd;
        decimal? amountArs = null, rate = null;
        if (currency == "USD")
            amountUsd = request.Amount;
        else if (currency == "ARS")
        {
            if (request.UsdRate is not > 0)
                throw new ValidationException("An amount in ARS needs the dollar rate of that day.");
            amountArs = request.Amount;
            rate = request.UsdRate;
            amountUsd = request.Amount / request.UsdRate.Value;
        }
        else
            throw new ValidationException("Currency must be USD or ARS.");

        db.InvestmentIncomes.Add(new InvestmentIncome
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HoldingId = holding.Id,
            Date = date,
            Type = request.Type,
            AmountUsd = amountUsd,
            AmountArs = amountArs,
            UsdRate = rate,
            CreatedAt = DateTime.UtcNow,
        });
        return HoldingOf(await SaveAndSnapshotAsync(userId), holding.Id);
    }

    public async Task<InvestmentHoldingDto> DeleteIncomeAsync(Guid userId, Guid incomeId)
    {
        var income = await db.InvestmentIncomes.FirstOrDefaultAsync(i => i.Id == incomeId && i.UserId == userId)
            ?? throw new NotFoundException("Income not found.");
        db.InvestmentIncomes.Remove(income);
        return HoldingOf(await SaveAndSnapshotAsync(userId), income.HoldingId);
    }

    private async Task<InvestmentHolding> LoadHoldingWithTradesAsync(Guid userId, Guid holdingId) =>
        await db.InvestmentHoldings.Include(h => h.Trades)
            .FirstOrDefaultAsync(h => h.Id == holdingId && h.UserId == userId)
        ?? throw new NotFoundException("Holding not found.");

    private static DateOnly ValidateDate(DateOnly? date)
    {
        var today = TodayInArgentina();
        var d = date ?? today;
        if (d > today)
            throw new ValidationException("The date can't be in the future.");
        if (d.Year < 2000)
            throw new ValidationException("Invalid date.");
        return d;
    }

    private static InvestmentTrade NewTrade(Guid userId, Guid holdingId, TradeInputDto input)
    {
        var date = ValidateDate(input.Date);
        if (input.Quantity <= 0)
            throw new ValidationException("The quantity must be greater than zero.");
        if (input.PriceArs < 0 || input.FeeArs < 0)
            throw new ValidationException("Price and fee can't be negative.");
        ValidateRate(input.UsdRate);
        if (!Enum.IsDefined(input.Type))
            throw new ValidationException("Invalid trade type.");
        return new InvestmentTrade
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            HoldingId = holdingId,
            Date = date,
            Type = input.Type,
            Quantity = input.Quantity,
            PriceArs = input.PriceArs,
            UsdRate = input.UsdRate,
            PriceUsd = input.PriceArs / input.UsdRate,
            FeeArs = input.FeeArs,
            FeeUsd = input.FeeArs / input.UsdRate,
            CreatedAt = DateTime.UtcNow,
        };
    }

    // Replays the trades in order: buys add to the quantity and to the cost (fees included,
    // so the PPC is a weighted average in USD); sells take units out at the current PPC and
    // book the difference with what they fetched (net of fees) as realized gain. A sell for
    // more than what was held at that date is rejected.
    internal static void Recalculate(InvestmentHolding holding)
    {
        decimal quantity = 0, cost = 0, realized = 0, invested = 0;
        foreach (var t in holding.Trades.OrderBy(t => t.Date).ThenBy(t => t.CreatedAt))
        {
            if (t.Type == InvestmentTradeType.Buy)
            {
                var spent = t.Quantity * t.PriceUsd + t.FeeUsd;
                quantity += t.Quantity;
                cost += spent;
                invested += spent;
            }
            else
            {
                if (t.Quantity > quantity)
                    throw new ValidationException(
                        $"On {t.Date:dd/MM/yyyy} you'd be selling {t.Quantity} {holding.Ticker} but only held {quantity} at that point.");
                var basis = cost / quantity * t.Quantity;
                realized += t.Quantity * t.PriceUsd - t.FeeUsd - basis;
                cost -= basis;
                quantity -= t.Quantity;
            }
        }
        holding.Quantity = quantity;
        holding.AverageCostUsd = quantity > 0 ? cost / quantity : 0;
        holding.RealizedGainUsd = realized;
        holding.InvestedUsd = invested;
    }

    private static string? NormalizeSymbol(string? symbol)
    {
        var s = symbol?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(s))
            return null;
        if (s.Length > 20 || !s.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-'))
            throw new ValidationException("Invalid market symbol.");
        return s;
    }

    public async Task DeleteHoldingAsync(Guid userId, Guid holdingId)
    {
        var holding = await db.InvestmentHoldings.FirstOrDefaultAsync(h => h.Id == holdingId && h.UserId == userId)
            ?? throw new NotFoundException("Holding not found.");
        db.InvestmentHoldings.Remove(holding);
        await SaveAndSnapshotAsync(userId);
    }

    private async Task<InvestmentPortfolio> GetOrCreatePortfolioAsync(Guid userId)
    {
        var portfolio = await db.InvestmentPortfolios.FirstOrDefaultAsync(p => p.UserId == userId);
        if (portfolio != null)
            return portfolio;
        portfolio = new InvestmentPortfolio
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetAmountUsd = 0,
            UsdRate = 0,
            UpdatedAt = DateTime.UtcNow,
        };
        db.InvestmentPortfolios.Add(portfolio);
        return portfolio;
    }

    private static void ValidateRate(decimal usdRate)
    {
        if (usdRate <= 0)
            throw new ValidationException("The dollar rate must be greater than zero.");
    }

    private static (string Name, string Color) ValidateGroup(InvestmentGroupRequestDto request)
    {
        var name = request.Name?.Trim() ?? "";
        if (name.Length is 0 or > 100)
            throw new ValidationException("The group name must have between 1 and 100 characters.");
        if (!Colors.Contains(request.Color))
            throw new ValidationException("Invalid color.");
        return (name, request.Color);
    }

    private async Task<string> ValidateHoldingAsync(Guid userId, InvestmentHoldingRequestDto request, Guid? holdingId)
    {
        var ticker = request.Ticker?.Trim().ToUpperInvariant() ?? "";
        if (ticker.Length is 0 or > 20)
            throw new ValidationException("The ticker must have between 1 and 20 characters.");
        if (request.TargetPercent is < 0 or > 100)
            throw new ValidationException("The target percent must be between 0 and 100.");
        if (request.PriceArs < 0)
            throw new ValidationException("The price can't be negative.");
        if (!await db.InvestmentGroups.AnyAsync(g => g.Id == request.GroupId && g.UserId == userId))
            throw new ValidationException("Invalid group.");
        if (await db.InvestmentHoldings.AnyAsync(h => h.UserId == userId && h.Ticker == ticker && h.Id != holdingId))
            throw new ConflictException($"You already have {ticker}.");
        return ticker;
    }

    private static InvestmentHoldingDto HoldingOf(InvestmentPortfolioDto portfolio, Guid holdingId) =>
        portfolio.Groups.SelectMany(g => g.Holdings).First(h => h.Id == holdingId);

    // Argentina has no DST, so a fixed offset gives its calendar day without relying on
    // the host's time zone database.
    private static DateOnly TodayInArgentina() =>
        DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));

    private async Task<InvestmentPortfolioDto> SaveAndSnapshotAsync(Guid userId)
    {
        await db.SaveChangesAsync();
        var portfolio = await GetPortfolioAsync(userId);
        await RecordSnapshotAsync(userId, portfolio);
        return portfolio;
    }

    // One snapshot per day: a later change the same day replaces it. Without a dollar rate
    // there's no USD value to record yet.
    private async Task RecordSnapshotAsync(Guid userId, InvestmentPortfolioDto portfolio)
    {
        if (portfolio.UsdRate <= 0)
            return;

        var today = TodayInArgentina();
        var snapshot = await db.InvestmentSnapshots
            .Include(x => x.Groups)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Date == today);
        if (snapshot == null)
        {
            snapshot = new InvestmentSnapshot
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Date = today,
                UsdRate = 0,
                ValueUsd = 0,
                CostUsd = 0,
            };
            db.InvestmentSnapshots.Add(snapshot);
        }
        else
        {
            db.InvestmentSnapshotGroups.RemoveRange(snapshot.Groups);
        }

        snapshot.UsdRate = portfolio.UsdRate;
        snapshot.ValueUsd = portfolio.ValueUsd;
        snapshot.CostUsd = portfolio.CostUsd;
        snapshot.RealizedGainUsd = portfolio.RealizedGainUsd;
        snapshot.IncomeUsd = portfolio.IncomeUsd;
        snapshot.InvestedUsd = portfolio.InvestedUsd;
        db.InvestmentSnapshotGroups.AddRange(portfolio.Groups.Select(g => new InvestmentSnapshotGroup
        {
            Id = Guid.NewGuid(),
            SnapshotId = snapshot.Id,
            Name = g.Name,
            Color = g.Color,
            ValueUsd = g.ValueUsd,
            CostUsd = g.CostUsd,
        }));
        await db.SaveChangesAsync();
    }

    public async Task<List<InvestmentSnapshotDto>> GetHistoryAsync(Guid userId, int months)
    {
        var query = db.InvestmentSnapshots.AsNoTracking().Where(x => x.UserId == userId);
        if (months > 0)
        {
            var from = TodayInArgentina().AddMonths(-Math.Min(months, 1200));
            query = query.Where(x => x.Date >= from);
        }
        return await query
            .OrderBy(x => x.Date)
            .Select(x => new InvestmentSnapshotDto
            {
                Date = x.Date,
                UsdRate = x.UsdRate,
                ValueUsd = x.ValueUsd,
                CostUsd = x.CostUsd,
                RealizedGainUsd = x.RealizedGainUsd,
                IncomeUsd = x.IncomeUsd,
                InvestedUsd = x.InvestedUsd,
                Groups = x.Groups
                    .OrderBy(g => g.Name)
                    .Select(g => new InvestmentSnapshotGroupDto
                    {
                        Name = g.Name,
                        Color = g.Color,
                        ValueUsd = g.ValueUsd,
                        CostUsd = g.CostUsd,
                    })
                    .ToList(),
            })
            .ToListAsync();
    }

    // Every derived value (USD price, value, gains, actual share) is computed here, never stored.
    private static InvestmentPortfolioDto BuildPortfolio(
        InvestmentPortfolio? portfolio, List<InvestmentGroup> groups, Dictionary<Guid, decimal> income)
    {
        var rate = portfolio?.UsdRate ?? 0;
        var target = portfolio?.TargetAmountUsd ?? 0;

        decimal PriceUsd(InvestmentHolding h) => rate > 0 ? h.PriceArs / rate : 0;
        var totalValue = groups.SelectMany(g => g.Holdings).Sum(h => h.Quantity * PriceUsd(h));
        decimal Share(decimal value) => totalValue > 0 ? value / totalValue * 100 : 0;
        decimal GainPct(decimal gain, decimal cost) => cost > 0 ? gain / cost * 100 : 0;

        var groupDtos = groups.Select(g =>
        {
            var holdings = g.Holdings
                .OrderBy(h => h.Ticker)
                .Select(h =>
                {
                    var priceUsd = PriceUsd(h);
                    var value = h.Quantity * priceUsd;
                    var cost = h.Quantity * h.AverageCostUsd;
                    var collected = income.GetValueOrDefault(h.Id);
                    var total = value - cost + h.RealizedGainUsd + collected;
                    return new InvestmentHoldingDto
                    {
                        Id = h.Id,
                        GroupId = h.GroupId,
                        Ticker = h.Ticker,
                        MarketSymbol = h.MarketSymbol,
                        AutoPrice = h.AutoPrice,
                        TargetPercent = h.TargetPercent,
                        TargetAmountUsd = target * h.TargetPercent / 100,
                        Quantity = h.Quantity,
                        AverageCostUsd = h.AverageCostUsd,
                        PriceArs = h.PriceArs,
                        PriceUsd = priceUsd,
                        PriceUpdatedAt = h.PriceUpdatedAt,
                        ValueUsd = value,
                        ActualPercent = Share(value),
                        CostUsd = cost,
                        GainUsd = value - cost,
                        GainPercent = GainPct(value - cost, cost),
                        RealizedGainUsd = h.RealizedGainUsd,
                        IncomeUsd = collected,
                        InvestedUsd = h.InvestedUsd,
                        TotalReturnUsd = total,
                        TotalReturnPercent = GainPct(total, h.InvestedUsd),
                    };
                })
                .ToList();
            var gValue = holdings.Sum(h => h.ValueUsd);
            var gCost = holdings.Sum(h => h.CostUsd);
            var gTarget = holdings.Sum(h => h.TargetPercent);
            var gInvested = holdings.Sum(h => h.InvestedUsd);
            var gTotal = holdings.Sum(h => h.TotalReturnUsd);
            return new InvestmentGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Color = g.Color,
                SortOrder = g.SortOrder,
                AllowFractional = g.AllowFractional,
                Holdings = holdings,
                TargetPercent = gTarget,
                TargetAmountUsd = target * gTarget / 100,
                ValueUsd = gValue,
                ActualPercent = Share(gValue),
                CostUsd = gCost,
                GainUsd = gValue - gCost,
                GainPercent = GainPct(gValue - gCost, gCost),
                RealizedGainUsd = holdings.Sum(h => h.RealizedGainUsd),
                IncomeUsd = holdings.Sum(h => h.IncomeUsd),
                InvestedUsd = gInvested,
                TotalReturnUsd = gTotal,
                TotalReturnPercent = GainPct(gTotal, gInvested),
            };
        }).ToList();

        var totalCost = groupDtos.Sum(g => g.CostUsd);
        var invested = groupDtos.Sum(g => g.InvestedUsd);
        var totalReturn = groupDtos.Sum(g => g.TotalReturnUsd);
        return new InvestmentPortfolioDto
        {
            TargetAmountUsd = target,
            UsdRate = rate,
            UpdatedAt = portfolio?.UpdatedAt,
            Groups = groupDtos,
            TargetPercent = groupDtos.Sum(g => g.TargetPercent),
            ValueUsd = totalValue,
            CostUsd = totalCost,
            GainUsd = totalValue - totalCost,
            GainPercent = GainPct(totalValue - totalCost, totalCost),
            RealizedGainUsd = groupDtos.Sum(g => g.RealizedGainUsd),
            IncomeUsd = groupDtos.Sum(g => g.IncomeUsd),
            InvestedUsd = invested,
            TotalReturnUsd = totalReturn,
            TotalReturnPercent = GainPct(totalReturn, invested),
        };
    }
}
