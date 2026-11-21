import { InvestmentPortfolio } from "../types/investment";

export interface ContributionLine {
  holdingId: string;
  ticker: string;
  units: number;
  amountUsd: number;
  targetPercent: number;
  percentAfter: number;
}

export interface ContributionPlan {
  lines: ContributionLine[];
  investedUsd: number;
  leftoverUsd: number;
}

const floorTo = (value: number, decimals: number) => {
  const f = 10 ** decimals;
  return Math.floor(value * f + 1e-9) / f;
};

// Buy-only rebalance: each holding's target is (current value + contribution) × its share
// of the target percents; the contribution covers the gaps, proportionally when it can't
// cover all of them. Units are whole unless the group allows fractions (2 decimals).
export function planContribution(portfolio: InvestmentPortfolio, amountUsd: number): ContributionPlan {
  const holdings = portfolio.groups.flatMap((g) =>
    g.holdings.map((h) => ({ ...h, fractional: g.allowFractional })),
  );
  const eligible = holdings.filter((h) => h.targetPercent > 0 && h.priceUsd > 0);
  const percentSum = eligible.reduce((s, h) => s + h.targetPercent, 0);
  if (amountUsd <= 0 || percentSum <= 0) {
    return { lines: [], investedUsd: 0, leftoverUsd: Math.max(amountUsd, 0) };
  }

  const newTotal = portfolio.valueUsd + amountUsd;
  const gaps = eligible.map((h) => Math.max(0, (newTotal * h.targetPercent) / percentSum - h.valueUsd));
  const gapSum = gaps.reduce((s, g) => s + g, 0);
  const scale = gapSum > amountUsd ? amountUsd / gapSum : 1;

  const units = eligible.map((h, i) => floorTo((gaps[i] * scale) / h.priceUsd, h.fractional ? 2 : 0));
  let leftover = amountUsd - eligible.reduce((s, h, i) => s + units[i] * h.priceUsd, 0);

  // Rounding down leaves money unassigned: spend it one whole unit at a time on whatever
  // is furthest below its target and still affordable.
  for (let guard = 0; guard < 1000; guard++) {
    let best = -1;
    let bestGap = 0;
    eligible.forEach((h, i) => {
      const remaining = gaps[i] - units[i] * h.priceUsd;
      if (!h.fractional && h.priceUsd <= leftover + 1e-9 && remaining > bestGap) {
        best = i;
        bestGap = remaining;
      }
    });
    if (best < 0) break;
    units[best] += 1;
    leftover -= eligible[best].priceUsd;
  }

  const investedUsd = amountUsd - leftover;
  const totalAfter = portfolio.valueUsd + investedUsd;
  const lines = eligible
    .map((h, i) => ({
      holdingId: h.id,
      ticker: h.ticker,
      units: units[i],
      amountUsd: units[i] * h.priceUsd,
      targetPercent: h.targetPercent,
      percentAfter: totalAfter > 0 ? ((h.valueUsd + units[i] * h.priceUsd) / totalAfter) * 100 : 0,
    }))
    .filter((l) => l.units > 0)
    .sort((a, b) => b.amountUsd - a.amountUsd);

  return { lines, investedUsd, leftoverUsd: Math.max(leftover, 0) };
}
