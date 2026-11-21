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
