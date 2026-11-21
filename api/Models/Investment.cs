namespace api.Models;
using System.ComponentModel.DataAnnotations;
using api.Models.Enums;

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
    // Symbol on the market (IOL/BYMA) when it differs from the ticker the user uses,
    // e.g. ticker "SP500" -> "SPY". Null means the ticker itself.
    public string? MarketSymbol { get; set; }
    // Whether "Traer cotizaciones" updates this holding's price.
    public bool AutoPrice { get; set; } = true;
    // Share of the target amount this holding should have, in percent (8.75 = 8.75%).
    public required decimal TargetPercent { get; set; }
    public required decimal PriceArs { get; set; }
    public required DateTime PriceUpdatedAt { get; set; }

    // Derived from the trades (InvestmentService.Recalculate) and stored so reads don't
    // replay the whole history. Never set directly.
    public required decimal Quantity { get; set; }
    public required decimal AverageCostUsd { get; set; }
    // Gains of what was sold: proceeds minus the average cost of the units sold.
    public decimal RealizedGainUsd { get; set; }
    // Everything ever paid for buys, fees included.
    public decimal InvestedUsd { get; set; }

    public List<InvestmentTrade> Trades { get; set; } = [];
    public List<InvestmentIncome> Incomes { get; set; } = [];
}

// A buy or a sell. Prices are entered in ARS with the dollar rate of that day and kept in
// USD too; the initial position created when trades were introduced only has the USD cost.
public class InvestmentTrade
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid HoldingId { get; set; }
    public InvestmentHolding Holding { get; set; } = null!;
    public required DateOnly Date { get; set; }
    public required InvestmentTradeType Type { get; set; }
    public required decimal Quantity { get; set; }
    public decimal? PriceArs { get; set; }
    public decimal? UsdRate { get; set; }
    public required decimal PriceUsd { get; set; }
    public decimal FeeArs { get; set; }
    public decimal FeeUsd { get; set; }
    // The position the holding had before trades existed (quantity and PPC as loaded by hand).
    public bool IsInitial { get; set; }
    public required DateTime CreatedAt { get; set; }
}

// Cash a holding paid: bond coupons and amortizations, dividends. Counts toward total return.
public class InvestmentIncome
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid HoldingId { get; set; }
    public InvestmentHolding Holding { get; set; } = null!;
    public required DateOnly Date { get; set; }
    public required InvestmentIncomeType Type { get; set; }
    public required decimal AmountUsd { get; set; }
    // Set when it was paid in pesos: the ARS amount and the rate used to convert it.
    public decimal? AmountArs { get; set; }
    public decimal? UsdRate { get; set; }
    public required DateTime CreatedAt { get; set; }
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
    // For the total return line. Snapshots taken before these existed have them at zero
    // (InvestedUsd zero means "unknown": the cost is the best estimate then).
    public decimal RealizedGainUsd { get; set; }
    public decimal IncomeUsd { get; set; }
    public decimal InvestedUsd { get; set; }
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
