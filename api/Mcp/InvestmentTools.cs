using System.ComponentModel;
using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using ModelContextProtocol.Server;
using static api.Mcp.McpErrors;

namespace api.Mcp;

// Investments portfolio (inversiones). Holdings and groups are addressed by ticker and name,
// which is what the user says, not by id. Deleting is left to the web UI on purpose.
[McpServerToolType]
public class InvestmentTools(IInvestmentService service, IHttpContextAccessor httpContextAccessor)
{
    private Guid CurrentUserId =>
        Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private const string MoneyNote =
        "Prices are in ARS as quoted by the broker; every value, cost and gain is in USD at the portfolio's dollar rate (usdRate, ARS per USD). The average cost (PPC) is stored in USD at the purchase-day rate.";

    [McpServerTool(Name = "get_portfolio", ReadOnly = true)]
    [Description($"""
        The whole investments portfolio: the amount the user plans to invest (targetAmountUsd), today's dollar rate, and each group (e.g. Oro, Bonos, Acciones) with its holdings. For each holding: ticker, target percent and the USD amount it should have, quantity, PPC, price in ARS and USD, current value, actual percent of the portfolio, cost, and gain in USD and percent. Group and portfolio totals included.
        {MoneyNote}
        Use it to answer how the investments are doing or how far each one is from its target.
        """)]
    public Task<object> GetPortfolio() =>
        Run(async () => (object)await service.GetPortfolioAsync(CurrentUserId));

    [McpServerTool(Name = "get_portfolio_history", ReadOnly = true)]
    [Description("Daily history of the portfolio: for each day with changes, the dollar rate, total value and cost in USD, and value and cost per group. One snapshot per day, recorded automatically whenever prices or holdings change. Use it for how the portfolio evolved over time.")]
    public Task<object> GetPortfolioHistory(
        [Description("How many months back. Omit for the whole history.")] int? months = null) =>
        Run(async () => (object)await service.GetHistoryAsync(CurrentUserId, months ?? 0));

    [McpServerTool(Name = "simulate_contribution", ReadOnly = true)]
    [Description("""
        What to buy with a new contribution to move the portfolio toward its target percents, at today's prices. Buy-only (never suggests selling). Returns, per holding, the units to buy, the amount in USD and ARS and the percent it would end at, plus the total invested and what is left unassigned. Units are whole unless the holding's group allows fractions (bonds). Nothing is saved.
        """)]
    public Task<object> SimulateContribution(
        [Description("Amount to invest, greater than zero")] decimal amount,
        [Description("Currency of the amount: USD or ARS. ARS is converted at the portfolio's dollar rate.")] string currency = "USD") =>
        Run(async () => (object)await service.SimulateContributionAsync(
            CurrentUserId, new SimulateContributionRequestDto { Amount = amount, Currency = currency }));

    [McpServerTool(Name = "update_investment_prices", Idempotent = true)]
    [Description("""
        The day-to-day update: sets today's dollar rate and the current price in ARS of one or more holdings, by ticker. Holdings not listed keep their price. Use it when the user shares prices from their broker.
        """)]
    public Task<object> UpdateInvestmentPrices(
        [Description("Prices in ARS by ticker, e.g. {\"GLD\": 13000, \"AL30\": 84000}. Tickers must already exist in the portfolio.")] Dictionary<string, decimal> prices,
        [Description("Today's dollar rate (ARS per USD). Omit to keep the current one.")] decimal? usdRate = null) =>
        Run(async () =>
        {
            var userId = CurrentUserId;
            var portfolio = await service.GetPortfolioAsync(userId);
            var byTicker = Holdings(portfolio).ToDictionary(h => h.Ticker, StringComparer.OrdinalIgnoreCase);
            var unknown = prices.Keys.Where(t => !byTicker.ContainsKey(t.Trim())).ToList();
            if (unknown.Count > 0)
                throw new NotFoundException($"Unknown tickers: {string.Join(", ", unknown)}. Existing: {string.Join(", ", byTicker.Keys)}.");
            if (prices.Count == 0 && usdRate == null)
                throw new ValidationException("Pass at least one price or the dollar rate.");

            return (object)await service.UpdatePricesAsync(userId, new UpdateInvestmentPricesRequestDto
            {
                UsdRate = usdRate ?? portfolio.UsdRate,
                Prices = prices
                    .Select(p => new InvestmentPriceDto { HoldingId = byTicker[p.Key.Trim()].Id, PriceArs = p.Value })
                    .ToList(),
            });
        });

    [McpServerTool(Name = "set_portfolio", Idempotent = true)]
    [Description("Sets the amount the user plans to invest in USD (it is split among holdings by their target percent) and/or today's dollar rate. Omitted values are kept.")]
    public Task<object> SetPortfolio(
        [Description("Planned total to invest, in USD")] decimal? targetAmountUsd = null,
        [Description("Today's dollar rate (ARS per USD)")] decimal? usdRate = null) =>
        Run(async () =>
        {
            var userId = CurrentUserId;
            var current = await service.GetPortfolioAsync(userId);
            return (object)await service.UpdatePortfolioAsync(userId, new UpdatePortfolioRequestDto
            {
                TargetAmountUsd = targetAmountUsd ?? current.TargetAmountUsd,
                UsdRate = usdRate ?? current.UsdRate,
            });
        });

    [McpServerTool(Name = "save_investment", Idempotent = true)]
    [Description("""
        Creates a holding, or updates it if the ticker already exists. On update, omitted fields keep their value; on create, group and priceArs are required.
        The PPC (average purchase price) can be given in USD (averageCostUsd) or in ARS together with the dollar rate of the purchase (averageCostArs + purchaseUsdRate); it is stored in USD.
        Confirm the numbers with the user before saving if they weren't explicit.
        """)]
    public Task<object> SaveInvestment(
        [Description("Ticker, e.g. GLD, AL30, SP500")] string ticker,
        [Description("Name of an existing group, e.g. Acciones. Use create_investment_group first if it doesn't exist.")] string? group = null,
        [Description("Target share of the planned investment, in percent (8.75 = 8.75%)")] decimal? targetPercent = null,
        [Description("Quantity held")] decimal? quantity = null,
        [Description("PPC in USD")] decimal? averageCostUsd = null,
        [Description("PPC in ARS; requires purchaseUsdRate")] decimal? averageCostArs = null,
        [Description("Dollar rate of the purchase day (ARS per USD), used with averageCostArs")] decimal? purchaseUsdRate = null,
        [Description("Current price in ARS")] decimal? priceArs = null) =>
        Run(async () =>
        {
            var userId = CurrentUserId;
            var portfolio = await service.GetPortfolioAsync(userId);
            var existing = Holdings(portfolio)
                .FirstOrDefault(h => h.Ticker.Equals(ticker.Trim(), StringComparison.OrdinalIgnoreCase));

            Guid groupId;
            if (group != null)
            {
                groupId = portfolio.Groups
                    .FirstOrDefault(g => g.Name.Equals(group.Trim(), StringComparison.OrdinalIgnoreCase))?.Id
                    ?? throw new NotFoundException($"Group \"{group}\" not found. Existing: {string.Join(", ", portfolio.Groups.Select(g => g.Name))}.");
            }
            else if (existing != null)
                groupId = existing.GroupId;
            else
                throw new ValidationException("group is required for a new holding.");

            if (averageCostUsd != null && averageCostArs != null)
                throw new ValidationException("Pass the PPC either in USD or in ARS, not both.");
            decimal? costUsd = averageCostUsd;
            if (averageCostArs != null)
            {
                if (purchaseUsdRate is not > 0)
                    throw new ValidationException("purchaseUsdRate is required with averageCostArs.");
                costUsd = averageCostArs / purchaseUsdRate;
            }

            if (existing == null && priceArs == null)
                throw new ValidationException("priceArs is required for a new holding.");

            var request = new InvestmentHoldingRequestDto
            {
                GroupId = groupId,
                Ticker = ticker,
                TargetPercent = targetPercent ?? existing?.TargetPercent ?? 0,
                Quantity = quantity ?? existing?.Quantity ?? 0,
                AverageCostUsd = costUsd ?? existing?.AverageCostUsd ?? 0,
                PriceArs = priceArs ?? existing!.PriceArs,
            };
            return existing == null
                ? (object)await service.CreateHoldingAsync(userId, request)
                : await service.UpdateHoldingAsync(userId, existing.Id, request);
        });

    [McpServerTool(Name = "create_investment_group")]
    [Description("Creates a group of holdings (e.g. Oro, Bonos a 20 años, Acciones).")]
    public Task<object> CreateInvestmentGroup(
        [Description("Group name")] string name,
        [Description("Color: amber, slate, green, red, blue, purple, teal or pink")] string color = "blue",
        [Description("True for bonds and other assets bought in fractional quantities; the contribution calculator then suggests fractions instead of whole units")] bool allowFractional = false) =>
        Run(async () => (object)await service.CreateGroupAsync(CurrentUserId, new InvestmentGroupRequestDto
        {
            Name = name,
            Color = color.Trim().ToLowerInvariant(),
            AllowFractional = allowFractional,
        }));

    private static IEnumerable<InvestmentHoldingDto> Holdings(InvestmentPortfolioDto portfolio) =>
        portfolio.Groups.SelectMany(g => g.Holdings);
}
