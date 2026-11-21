using api.Models.Enums;

namespace api.DTOs;

public class InvestmentPortfolioDto
{
    public required decimal TargetAmountUsd { get; set; }
    public required decimal UsdRate { get; set; }
    public required DateTime? UpdatedAt { get; set; }
    public required List<InvestmentGroupDto> Groups { get; set; }
    // Sum of every holding's target percent; it should be 100.
    public required decimal TargetPercent { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal CostUsd { get; set; }
    public required decimal GainUsd { get; set; }
    public required decimal GainPercent { get; set; }
    // Total return = unrealized gain (GainUsd) + realized gains + income collected;
    // its percent is over everything ever invested.
    public required decimal RealizedGainUsd { get; set; }
    public required decimal IncomeUsd { get; set; }
    public required decimal InvestedUsd { get; set; }
    public required decimal TotalReturnUsd { get; set; }
    public required decimal TotalReturnPercent { get; set; }
}

public class InvestmentGroupDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required int SortOrder { get; set; }
    public required bool AllowFractional { get; set; }
    public required List<InvestmentHoldingDto> Holdings { get; set; }
    public required decimal TargetPercent { get; set; }
    public required decimal TargetAmountUsd { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal ActualPercent { get; set; }
    public required decimal CostUsd { get; set; }
    public required decimal GainUsd { get; set; }
    public required decimal GainPercent { get; set; }
    // Total return = unrealized gain (GainUsd) + realized gains + income collected;
    // its percent is over everything ever invested.
    public required decimal RealizedGainUsd { get; set; }
    public required decimal IncomeUsd { get; set; }
    public required decimal InvestedUsd { get; set; }
    public required decimal TotalReturnUsd { get; set; }
    public required decimal TotalReturnPercent { get; set; }
}

public class InvestmentHoldingDto
{
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }
    public required string Ticker { get; set; }
    public required string? MarketSymbol { get; set; }
    public required bool AutoPrice { get; set; }
    public required decimal TargetPercent { get; set; }
    public required decimal TargetAmountUsd { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal AverageCostUsd { get; set; }
    public required decimal PriceArs { get; set; }
    public required decimal PriceUsd { get; set; }
    public required DateTime PriceUpdatedAt { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal ActualPercent { get; set; }
    public required decimal CostUsd { get; set; }
    public required decimal GainUsd { get; set; }
    public required decimal GainPercent { get; set; }
    // Total return = unrealized gain (GainUsd) + realized gains + income collected;
    // its percent is over everything ever invested.
    public required decimal RealizedGainUsd { get; set; }
    public required decimal IncomeUsd { get; set; }
    public required decimal InvestedUsd { get; set; }
    public required decimal TotalReturnUsd { get; set; }
    public required decimal TotalReturnPercent { get; set; }
}

public class UpdatePortfolioRequestDto
{
    public required decimal TargetAmountUsd { get; set; }
    public required decimal UsdRate { get; set; }
}

public class InvestmentGroupRequestDto
{
    public required string Name { get; set; }
    public required string Color { get; set; }
    public bool AllowFractional { get; set; }
}

// Quantity and PPC aren't part of it: they come from the trades. A new holding can bring
// its first buy along.
public class InvestmentHoldingRequestDto
{
    public required Guid GroupId { get; set; }
    public required string Ticker { get; set; }
    public string? MarketSymbol { get; set; }
    public bool AutoPrice { get; set; } = true;
    public required decimal TargetPercent { get; set; }
    public required decimal PriceArs { get; set; }
    public TradeInputDto? InitialTrade { get; set; }
}

public class TradeInputDto
{
    // Defaults to today (Argentina).
    public DateOnly? Date { get; set; }
    public required InvestmentTradeType Type { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal PriceArs { get; set; }
    public required decimal UsdRate { get; set; }
    public decimal FeeArs { get; set; }
}

public class InvestmentTradeRequestDto : TradeInputDto
{
    public required Guid HoldingId { get; set; }
}

public class InvestmentIncomeRequestDto
{
    public required Guid HoldingId { get; set; }
    public DateOnly? Date { get; set; }
    public required InvestmentIncomeType Type { get; set; }
    public required decimal Amount { get; set; }
    // "USD" or "ARS"; ARS needs the rate of the day to be stored in USD.
    public required string Currency { get; set; }
    public decimal? UsdRate { get; set; }
}

public class InvestmentTradeDto
{
    public required Guid Id { get; set; }
    public required Guid HoldingId { get; set; }
    public required string Ticker { get; set; }
    public required DateOnly Date { get; set; }
    public required InvestmentTradeType Type { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal? PriceArs { get; set; }
    public required decimal? UsdRate { get; set; }
    public required decimal PriceUsd { get; set; }
    public required decimal FeeArs { get; set; }
    public required decimal FeeUsd { get; set; }
    public required bool IsInitial { get; set; }
}

public class InvestmentIncomeDto
{
    public required Guid Id { get; set; }
    public required Guid HoldingId { get; set; }
    public required string Ticker { get; set; }
    public required DateOnly Date { get; set; }
    public required InvestmentIncomeType Type { get; set; }
    public required decimal AmountUsd { get; set; }
    public required decimal? AmountArs { get; set; }
    public required decimal? UsdRate { get; set; }
}

public class InvestmentActivityDto
{
    public required List<InvestmentTradeDto> Trades { get; set; }
    public required List<InvestmentIncomeDto> Incomes { get; set; }
}

public class UpdateInvestmentPricesRequestDto
{
    public required decimal UsdRate { get; set; }
    public required List<InvestmentPriceDto> Prices { get; set; }
}

public class InvestmentPriceDto
{
    public required Guid HoldingId { get; set; }
    public required decimal PriceArs { get; set; }
}

public class InvestmentSnapshotDto
{
    public required DateOnly Date { get; set; }
    public required decimal UsdRate { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal CostUsd { get; set; }
    public required decimal RealizedGainUsd { get; set; }
    public required decimal IncomeUsd { get; set; }
    public required decimal InvestedUsd { get; set; }
    public required List<InvestmentSnapshotGroupDto> Groups { get; set; }
}

public class InvestmentSnapshotGroupDto
{
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal CostUsd { get; set; }
}

public class SimulateContributionRequestDto
{
    public required decimal Amount { get; set; }
    // "USD" or "ARS"; ARS is converted at the portfolio's dollar rate.
    public required string Currency { get; set; }
}

public class ContributionPlanDto
{
    public required decimal AmountUsd { get; set; }
    public required decimal UsdRate { get; set; }
    public required decimal InvestedUsd { get; set; }
    public required decimal LeftoverUsd { get; set; }
    public required List<ContributionLineDto> Lines { get; set; }
}

public class ContributionLineDto
{
    public required Guid HoldingId { get; set; }
    public required string Ticker { get; set; }
    public required decimal Units { get; set; }
    public required decimal AmountUsd { get; set; }
    public required decimal AmountArs { get; set; }
    public required decimal TargetPercent { get; set; }
    public required decimal PercentAfter { get; set; }
}

// IOL credentials are used for this one request to get a token and are never stored or logged.
public class IolQuotesRequestDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class QuotesResultDto
{
    public required List<QuoteDto> Updated { get; set; }
    public required List<QuoteMissDto> NotFound { get; set; }
    // Null when the dollar couldn't be fetched (the previous rate is kept).
    public required decimal? UsdRate { get; set; }
    public required InvestmentPortfolioDto Portfolio { get; set; }
}

public class QuoteDto
{
    public required string Ticker { get; set; }
    public required string Symbol { get; set; }
    public required decimal PriceArs { get; set; }
}

public class QuoteMissDto
{
    public required string Ticker { get; set; }
    public required string Symbol { get; set; }
    public required string Reason { get; set; }
}
