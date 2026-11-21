using api.Exceptions;
using api.Models;
using api.Models.Enums;
using api.Services;

namespace api.Tests;

// Quantity, PPC (USD, fees included), realized gain and total invested are replayed from the
// trades by InvestmentService.Recalculate.
public class RecalculateTests
{
    private static readonly DateTime T0 = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private int _seq;

    private static InvestmentHolding Holding(params InvestmentTrade[] trades)
    {
        var h = new InvestmentHolding
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            GroupId = Guid.NewGuid(),
            Ticker = "GLD",
            TargetPercent = 10,
            PriceArs = 0,
            PriceUpdatedAt = T0,
            Quantity = 0,
            AverageCostUsd = 0,
        };
        h.Trades.AddRange(trades);
        return h;
    }

    // Prices entered in ARS with the day's rate, like the app does.
    private InvestmentTrade Trade(InvestmentTradeType type, string date, decimal qty, decimal priceArs, decimal rate, decimal feeArs = 0) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.Empty,
        HoldingId = Guid.Empty,
        Date = DateOnly.Parse(date),
        Type = type,
        Quantity = qty,
        PriceArs = priceArs,
        UsdRate = rate,
        PriceUsd = priceArs / rate,
        FeeArs = feeArs,
        FeeUsd = feeArs / rate,
        CreatedAt = T0.AddSeconds(_seq++),
    };

    private InvestmentTrade Buy(string date, decimal qty, decimal priceArs, decimal rate, decimal feeArs = 0) =>
        Trade(InvestmentTradeType.Buy, date, qty, priceArs, rate, feeArs);

    private InvestmentTrade Sell(string date, decimal qty, decimal priceArs, decimal rate, decimal feeArs = 0) =>
        Trade(InvestmentTradeType.Sell, date, qty, priceArs, rate, feeArs);

    private static void AssertClose(decimal expected, decimal actual) =>
        Assert.True(Math.Abs(expected - actual) < 0.000001m, $"Expected {expected}, got {actual}");

    [Fact]
    public void NoTrades_LeavesEverythingAtZero()
    {
        var h = Holding();
        InvestmentService.Recalculate(h);
        Assert.Equal(0, h.Quantity);
        Assert.Equal(0, h.AverageCostUsd);
        Assert.Equal(0, h.RealizedGainUsd);
        Assert.Equal(0, h.InvestedUsd);
    }

    [Fact]
    public void Buy_FeeIsAddedToCost()
    {
        // 10 × 12000 ARS at 1600 = 75 USD, plus a 1600 ARS fee = 1 USD -> PPC 7.6
        var h = Holding(Buy("2026-01-10", 10, 12000, 1600, feeArs: 1600));
        InvestmentService.Recalculate(h);
        Assert.Equal(10, h.Quantity);
        AssertClose(7.6m, h.AverageCostUsd);
        AssertClose(76m, h.InvestedUsd);
    }

    [Fact]
    public void SeveralBuys_AverageThePpcInUsd()
    {
        // 10 @ 10 USD and 30 @ 14 USD (different rates) -> (100 + 420) / 40 = 13
        var h = Holding(
            Buy("2026-01-10", 10, 15000, 1500),
            Buy("2026-02-10", 30, 23800, 1700));
        InvestmentService.Recalculate(h);
        Assert.Equal(40, h.Quantity);
        AssertClose(13m, h.AverageCostUsd);
        AssertClose(520m, h.InvestedUsd);
    }

    [Fact]
    public void PartialSell_KeepsPpcAndBooksRealizedGain()
    {
        // PPC 10; sell 4 @ 15 USD with a 2 USD fee -> 60 - 2 - 40 = 18
        var h = Holding(
            Buy("2026-01-10", 10, 16000, 1600),
            Sell("2026-03-01", 4, 30000, 2000, feeArs: 4000));
        InvestmentService.Recalculate(h);
        Assert.Equal(6, h.Quantity);
        AssertClose(10m, h.AverageCostUsd);
        AssertClose(18m, h.RealizedGainUsd);
        AssertClose(100m, h.InvestedUsd);
    }

    [Fact]
    public void SellingAtALoss_BooksANegativeRealizedGain()
    {
        var h = Holding(
            Buy("2026-01-10", 10, 16000, 1600),
            Sell("2026-03-01", 5, 12000, 2000));
        InvestmentService.Recalculate(h);
        AssertClose(-20m, h.RealizedGainUsd); // 5 × 6 - 5 × 10
    }

    [Fact]
    public void SellingEverything_LeavesQuantityAndPpcAtZero()
    {
        var h = Holding(
            Buy("2026-01-10", 10, 16000, 1600),
            Sell("2026-03-01", 10, 22000, 2000));
        InvestmentService.Recalculate(h);
        Assert.Equal(0, h.Quantity);
        Assert.Equal(0, h.AverageCostUsd);
        AssertClose(10m, h.RealizedGainUsd); // 10 × 11 - 100
    }

    [Fact]
    public void BuyAfterASell_AveragesOnlyWhatIsLeft()
    {
        // 10 @ 10, sell 5 (cost left 50), buy 5 @ 20 -> (50 + 100) / 10 = 15
        var h = Holding(
            Buy("2026-01-10", 10, 16000, 1600),
            Sell("2026-02-01", 5, 20000, 1600),
            Buy("2026-03-01", 5, 32000, 1600));
        InvestmentService.Recalculate(h);
        Assert.Equal(10, h.Quantity);
        AssertClose(15m, h.AverageCostUsd);
        AssertClose(200m, h.InvestedUsd);
    }

    [Fact]
    public void SellingMoreThanHeld_IsRejected()
    {
        var h = Holding(
            Buy("2026-01-10", 10, 16000, 1600),
            Sell("2026-03-01", 11, 16000, 1600));
        var ex = Assert.Throws<ValidationException>(() => InvestmentService.Recalculate(h));
        Assert.Contains("GLD", ex.Message);
    }

    [Fact]
    public void ASellDatedBeforeTheBuy_IsRejectedEvenIfRegisteredLater()
    {
        // Registration order doesn't matter: trades are replayed by date.
        var h = Holding(
            Buy("2026-03-01", 10, 16000, 1600),
            Sell("2026-02-01", 5, 16000, 1600));
        Assert.Throws<ValidationException>(() => InvestmentService.Recalculate(h));
    }

    [Fact]
    public void DeletingTheBuyBehindASell_IsRejected()
    {
        var buy = Buy("2026-01-10", 10, 16000, 1600);
        var h = Holding(buy, Sell("2026-03-01", 5, 16000, 1600));
        InvestmentService.Recalculate(h);

        h.Trades.Remove(buy);
        Assert.Throws<ValidationException>(() => InvestmentService.Recalculate(h));
    }

    [Fact]
    public void SameDay_UsesRegistrationOrder()
    {
        var h = Holding(
            Buy("2026-01-10", 10, 16000, 1600),
            Sell("2026-01-10", 10, 16000, 1600));
        InvestmentService.Recalculate(h);
        Assert.Equal(0, h.Quantity);
    }

    [Fact]
    public void InitialPosition_WithOnlyAUsdCost_Works()
    {
        // The migration's initial positions have no ARS price nor rate.
        var initial = new InvestmentTrade
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Empty,
            HoldingId = Guid.Empty,
            Date = DateOnly.Parse("2026-01-01"),
            Type = InvestmentTradeType.Buy,
            Quantity = 61,
            PriceUsd = 7.28m,
            IsInitial = true,
            CreatedAt = T0,
        };
        var h = Holding(initial, Buy("2026-02-01", 10, 12000, 1600));
        InvestmentService.Recalculate(h);
        Assert.Equal(71, h.Quantity);
        AssertClose((61 * 7.28m + 75m) / 71, h.AverageCostUsd);
    }

    [Fact]
    public void FractionalBondQuantities_Work()
    {
        // Bonds are held in hundreds of nominal value: 1.33 × 84040 ARS at 1655
        var h = Holding(
            Buy("2026-01-10", 1.33m, 84040, 1655),
            Sell("2026-02-10", 0.5m, 90000, 1700));
        InvestmentService.Recalculate(h);
        Assert.Equal(0.83m, h.Quantity);
        AssertClose(84040m / 1655, h.AverageCostUsd);
        AssertClose(0.5m * (90000m / 1700 - 84040m / 1655), h.RealizedGainUsd);
    }
}
