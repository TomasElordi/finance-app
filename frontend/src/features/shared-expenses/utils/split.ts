// Split math works in cents so the parts always add up exactly to the total.

export type SplitMode = "equal" | "percent" | "amount";

export const SPLIT_MODE_LABELS: Record<SplitMode, string> = {
  equal: "Partes iguales",
  percent: "Porcentaje",
  amount: "Montos exactos",
};

export function toCents(value: string | number): number {
  const n = typeof value === "number" ? value : Number(value.replace(",", "."));
  return Number.isFinite(n) ? Math.round(n * 100) : NaN;
}

// Spreads totalCents proportionally to weights; leftover cents go to the largest
// weights first, so 100 between three people is 33.34 / 33.33 / 33.33.
export function distribute(totalCents: number, weights: number[]): number[] {
  const weightSum = weights.reduce((a, b) => a + b, 0);
  if (weightSum <= 0) return weights.map(() => 0);
  const raw = weights.map((w) => (totalCents * w) / weightSum);
  const parts = raw.map(Math.floor);
  let leftover = totalCents - parts.reduce((a, b) => a + b, 0);
  const order = raw
    .map((r, i) => ({ i, frac: r - Math.floor(r), w: weights[i] }))
    .sort((a, b) => b.frac - a.frac || b.w - a.w);
  for (let k = 0; leftover > 0 && k < order.length; k++, leftover--) {
    if (weights[order[k].i] > 0) parts[order[k].i]++;
    else leftover++;
  }
  return parts;
}

// Picks the mode that reproduces a saved split, so editing shows "70/30"
// instead of raw amounts when that's how it was entered.
export function detectMode(
  totalCents: number,
  partsCents: number[],
): { mode: SplitMode; percents: number[] } {
  const participants = partsCents.map((p) => (p > 0 ? 1 : 0));
  const equal = distribute(totalCents, participants);
  if (equal.every((p, i) => p === partsCents[i])) {
    return { mode: "equal", percents: [] };
  }
  const percents = partsCents.map((p) => Math.round((p / totalCents) * 100));
  const fromPercents = distribute(totalCents, percents);
  if (percents.reduce((a, b) => a + b, 0) === 100 && fromPercents.every((p, i) => p === partsCents[i])) {
    return { mode: "percent", percents };
  }
  return { mode: "amount", percents: [] };
}

export function todayIso(): string {
  const d = new Date();
  return isoDate(d.getFullYear(), d.getMonth() + 1, d.getDate());
}

export function isoDate(year: number, month: number, day: number): string {
  return `${year}-${String(month).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
}

// Default date for a new record while viewing year/month: today if it's that
// month, otherwise the first day of it.
export function defaultDateFor(year: number, month: number): string {
  const now = new Date();
  return now.getFullYear() === year && now.getMonth() + 1 === month ? todayIso() : isoDate(year, month, 1);
}

export function formatShortDate(iso: string): string {
  const [, m, d] = iso.split("-");
  return `${d}/${m}`;
}
