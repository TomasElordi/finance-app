using api.DTOs;
using api.Services;

namespace api.Tests;

// The buy-only rebalance behind "Simular aporte" and the simulate_contribution MCP tool.
public class PlanContributionTests
{
    private record H(string Ticker, decimal TargetPercent, decimal PriceUsd, decimal ValueUsd = 0, bool Fractional = false);

    private const decimal Rate = 1000;

    // Builds a portfolio DTO from the few numbers the calculation reads.
    private static InvestmentPortfolioDto Portfolio(params H[] holdings)
    {
        var groups = holdings
            .GroupBy(h => h.Fractional)
            .Select(g => new InvestmentGroupDto
            {
                Id = Guid.NewGuid(),
                Name = g.Key ? "Bonos" : "Acciones",
                Color = "blue",
                SortOrder = 0,
                AllowFractional = g.Key,
                Holdings = g.Select(h => new InvestmentHoldingDto
                {
                    Id = Guid.NewGuid(),
                    GroupId = Guid.Empty,
                    Ticker = h.Ticker,
                    MarketSymbol = null,
                    AutoPrice = true,
                    TargetPercent = h.TargetPercent,
                    TargetAmountUsd = 0,
                    Quantity = h.PriceUsd > 0 ? h.ValueUsd / h.PriceUsd : 0,
                    AverageCostUsd = 0,
                    PriceArs = h.PriceUsd * Rate,
                    PriceUsd = h.PriceUsd,
                    PriceUpdatedAt = DateTime.UtcNow,
                    ValueUsd = h.ValueUsd,
                    ActualPercent = 0,
                    CostUsd = 0,
                    GainUsd = 0,
                    GainPercent = 0,
                    RealizedGainUsd = 0,
                    IncomeUsd = 0,
                    InvestedUsd = 0,
                    TotalReturnUsd = 0,
                    TotalReturnPercent = 0,
                }).ToList(),
                TargetPercent = 0,
                TargetAmountUsd = 0,
                ValueUsd = 0,
                ActualPercent = 0,
                CostUsd = 0,
                GainUsd = 0,
                GainPercent = 0,
                RealizedGainUsd = 0,
                IncomeUsd = 0,
                InvestedUsd = 0,
                TotalReturnUsd = 0,
                TotalReturnPercent = 0,
            })
            .ToList();
        return new InvestmentPortfolioDto
        {
            TargetAmountUsd = 0,
            UsdRate = Rate,
            UpdatedAt = null,
            Groups = groups,
            TargetPercent = holdings.Sum(h => h.TargetPercent),
            ValueUsd = holdings.Sum(h => h.ValueUsd),
            CostUsd = 0,
            GainUsd = 0,
            GainPercent = 0,
            RealizedGainUsd = 0,
            IncomeUsd = 0,
            InvestedUsd = 0,
            TotalReturnUsd = 0,
            TotalReturnPercent = 0,
        };
    }

    private static ContributionLineDto? Line(ContributionPlanDto plan, string ticker) =>
        plan.Lines.SingleOrDefault(l => l.Ticker == ticker);

    [Fact]
    public void EmptyPortfolio_SplitsByTargetPercent()
    {
        var plan = InvestmentService.PlanContribution(
            Portfolio(new H("A", 60, 10), new H("B", 40, 10)), 1000, "USD");
        Assert.Equal(60, Line(plan, "A")!.Units);
        Assert.Equal(40, Line(plan, "B")!.Units);
        Assert.Equal(1000, plan.InvestedUsd);
        Assert.Equal(0, plan.LeftoverUsd);
    }

    [Fact]
    public void NeverSells_AndOverweightHoldingsGetNothing()
    {
        // A is way above its 50%; everything goes to B.
        var plan = InvestmentService.PlanContribution(
            Portfolio(new H("A", 50, 10, ValueUsd: 900), new H("B", 50, 10, ValueUsd: 100)), 200, "USD");
        Assert.Null(Line(plan, "A"));
        Assert.Equal(20, Line(plan, "B")!.Units);
        Assert.All(plan.Lines, l => Assert.True(l.Units > 0));
    }

    [Fact]
    public void ContributionSmallerThanTheGaps_IsSplitProportionally()
    {
        // Two underweight holdings and an overweight one; 100 USD can't cover both gaps.
        var plan = InvestmentService.PlanContribution(
            Portfolio(
                new H("A", 40, 1, ValueUsd: 300),
                new H("B", 40, 1, ValueUsd: 300),
                new H("C", 20, 1, ValueUsd: 400)),
            100, "USD");
        // New total 1100: A and B target 440 each (gap 140 each, 280 total) -> 100 split 50/50.
        Assert.Equal(50, Line(plan, "A")!.Units);
        Assert.Equal(50, Line(plan, "B")!.Units);
        Assert.Null(Line(plan, "C"));
    }

    [Fact]
    public void WholeUnits_RoundDown_AndTheLeftoverIsReported()
    {
        // 25 USD into a single 10 USD holding -> 2 units, 5 left.
        var plan = InvestmentService.PlanContribution(Portfolio(new H("A", 100, 10)), 25, "USD");
        Assert.Equal(2, Line(plan, "A")!.Units);
        Assert.Equal(20, plan.InvestedUsd);
        Assert.Equal(5, plan.LeftoverUsd);
    }

    [Fact]
    public void FractionalGroups_GetTwoDecimals()
    {
        var plan = InvestmentService.PlanContribution(Portfolio(new H("AL30", 100, 50, Fractional: true)), 100.99m, "USD");
        Assert.Equal(2.01m, Line(plan, "AL30")!.Units); // 100.99 / 50 = 2.0198 -> 2.01
        Assert.True(plan.LeftoverUsd < 0.5m);
    }

    [Fact]
    public void Leftover_IsSpentOnTheFurthestBelowTargetThatIsAffordable()
    {
        // 30 USD, A and B 50% each at 20 USD: proportional gives 0.75 units each -> 0 whole units.
        // The leftover pass then buys 1 unit of one of them (20 USD) and leaves 10.
        var plan = InvestmentService.PlanContribution(
            Portfolio(new H("A", 50, 20), new H("B", 50, 20)), 30, "USD");
        Assert.Single(plan.Lines);
        Assert.Equal(1, plan.Lines[0].Units);
        Assert.Equal(20, plan.InvestedUsd);
        Assert.Equal(10, plan.LeftoverUsd);
    }

    [Fact]
    public void NotEnoughForASingleUnit_SuggestsNothing()
    {
        var plan = InvestmentService.PlanContribution(Portfolio(new H("A", 100, 500)), 100, "USD");
        Assert.Empty(plan.Lines);
        Assert.Equal(0, plan.InvestedUsd);
        Assert.Equal(100, plan.LeftoverUsd);
    }

    [Fact]
    public void HoldingsWithoutPriceOrTarget_AreIgnored()
    {
        var plan = InvestmentService.PlanContribution(
            Portfolio(new H("A", 50, 10), new H("NOPRICE", 50, 0), new H("NOTARGET", 0, 10)), 100, "USD");
        Assert.Equal(["A"], plan.Lines.Select(l => l.Ticker));
        // Targets are normalized over the eligible holdings, so A gets everything.
        Assert.Equal(10, Line(plan, "A")!.Units);
    }

    [Fact]
    public void NoEligibleHoldings_LeavesEverythingUnassigned()
    {
        var plan = InvestmentService.PlanContribution(Portfolio(new H("A", 0, 10)), 100, "USD");
        Assert.Empty(plan.Lines);
        Assert.Equal(100, plan.LeftoverUsd);
    }

    [Fact]
    public void ArsAmount_IsConvertedAtThePortfolioRate()
    {
        // 50 000 ARS at 1000 = 50 USD -> 5 units of a 10 USD holding.
        var plan = InvestmentService.PlanContribution(Portfolio(new H("A", 100, 10)), 50_000, "ARS");
        Assert.Equal(50, plan.AmountUsd);
        Assert.Equal(5, Line(plan, "A")!.Units);
        Assert.Equal(50_000, Line(plan, "A")!.AmountArs);
    }

    [Fact]
    public void PercentAfter_ReflectsThePortfolioAfterBuying()
    {
        var plan = InvestmentService.PlanContribution(
            Portfolio(new H("A", 50, 10, ValueUsd: 100), new H("B", 50, 10, ValueUsd: 0)), 100, "USD");
        // B gets the 100 -> both at 100 of 200.
        Assert.Equal(50, Line(plan, "B")!.PercentAfter);
        Assert.True(plan.InvestedUsd + plan.LeftoverUsd == 100);
    }
}
