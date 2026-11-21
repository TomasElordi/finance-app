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
    // Bonds are bought in fractional quantities; stocks and CEDEARs in whole units.
    // Used by the contribution calculator to round the suggested quantities.
    public bool AllowFractional { get; set; }
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

// Daily photo of the portfolio, taken automatically after any change. Groups are copied by
// name and color (not referenced) so deleting or renaming a group doesn't rewrite history.
public class InvestmentSnapshot
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    // Argentina's calendar day.
    public required DateOnly Date { get; set; }
    public required decimal UsdRate { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal CostUsd { get; set; }
    public List<InvestmentSnapshotGroup> Groups { get; set; } = [];
}

public class InvestmentSnapshotGroup
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid SnapshotId { get; set; }
    public InvestmentSnapshot Snapshot { get; set; } = null!;
    public required string Name { get; set; }
    public required string Color { get; set; }
    public required decimal ValueUsd { get; set; }
    public required decimal CostUsd { get; set; }
}
