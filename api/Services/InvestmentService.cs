using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
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
        return BuildPortfolio(portfolio, groups);
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
            TargetPercent = request.TargetPercent,
            Quantity = request.Quantity,
            AverageCostUsd = request.AverageCostUsd,
            PriceArs = request.PriceArs,
            PriceUpdatedAt = DateTime.UtcNow,
        };
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
        holding.TargetPercent = request.TargetPercent;
        holding.Quantity = request.Quantity;
        holding.AverageCostUsd = request.AverageCostUsd;
        holding.PriceArs = request.PriceArs;
        return HoldingOf(await SaveAndSnapshotAsync(userId), holding.Id);
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
        if (request.Quantity < 0 || request.AverageCostUsd < 0 || request.PriceArs < 0)
            throw new ValidationException("Quantity, cost and price can't be negative.");
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
    private static InvestmentPortfolioDto BuildPortfolio(InvestmentPortfolio? portfolio, List<InvestmentGroup> groups)
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
                    return new InvestmentHoldingDto
                    {
                        Id = h.Id,
                        GroupId = h.GroupId,
                        Ticker = h.Ticker,
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
                    };
                })
                .ToList();
            var gValue = holdings.Sum(h => h.ValueUsd);
            var gCost = holdings.Sum(h => h.CostUsd);
            var gTarget = holdings.Sum(h => h.TargetPercent);
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
            };
        }).ToList();

        var totalCost = groupDtos.Sum(g => g.CostUsd);
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
        };
    }
}
