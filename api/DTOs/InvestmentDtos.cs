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
}

public class InvestmentHoldingDto
{
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }
    public required string Ticker { get; set; }
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

public class InvestmentHoldingRequestDto
{
    public required Guid GroupId { get; set; }
    public required string Ticker { get; set; }
    public required decimal TargetPercent { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal AverageCostUsd { get; set; }
    public required decimal PriceArs { get; set; }
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
