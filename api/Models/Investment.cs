namespace api.Models;
using System.ComponentModel.DataAnnotations;

// Investments are a standalone tool: they keep their own records and never generate entries.
// Prices are loaded in ARS (as quoted by the broker) and shown in USD using the portfolio's
// dollar rate; the average cost (PPC) is stored already converted to USD at the rate of the
// purchase day, so gains in USD aren't distorted by devaluation.

// One per user: the planned total to invest and the current dollar rate.
public class InvestmentPortfolio
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required decimal TargetAmountUsd { get; set; }
    // ARS per USD.
    public required decimal UsdRate { get; set; }
    public required DateTime UpdatedAt { get; set; }
}

// "Tipo de acción": Oro, Commodities, Bonos, Acciones...
public class InvestmentGroup
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required int SortOrder { get; set; }
    public List<InvestmentHolding> Holdings { get; set; } = [];
}

public class InvestmentHolding
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required Guid GroupId { get; set; }
    public InvestmentGroup Group { get; set; } = null!;
    public required string Ticker { get; set; }
    // Share of the target amount this holding should have, in percent (8.75 = 8.75%).
    public required decimal TargetPercent { get; set; }
    public required decimal Quantity { get; set; }
    public required decimal AverageCostUsd { get; set; }
    public required decimal PriceArs { get; set; }
    public required DateTime PriceUpdatedAt { get; set; }
}
