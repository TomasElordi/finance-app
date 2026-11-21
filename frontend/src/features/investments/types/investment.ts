export type GroupColor = "amber" | "slate" | "green" | "red" | "blue" | "purple" | "teal" | "pink";

export interface InvestmentHolding {
  id: string;
  groupId: string;
  ticker: string;
  targetPercent: number;
  targetAmountUsd: number;
  quantity: number;
  averageCostUsd: number;
  priceArs: number;
  priceUsd: number;
  priceUpdatedAt: string;
  valueUsd: number;
  actualPercent: number;
  costUsd: number;
  gainUsd: number;
  gainPercent: number;
}

export interface InvestmentGroup {
  id: string;
  name: string;
  color: GroupColor;
  sortOrder: number;
  allowFractional: boolean;
  holdings: InvestmentHolding[];
  targetPercent: number;
  targetAmountUsd: number;
  valueUsd: number;
  actualPercent: number;
  costUsd: number;
  gainUsd: number;
  gainPercent: number;
}

export interface InvestmentPortfolio {
  targetAmountUsd: number;
  usdRate: number;
  updatedAt: string | null;
  groups: InvestmentGroup[];
  targetPercent: number;
  valueUsd: number;
  costUsd: number;
  gainUsd: number;
  gainPercent: number;
}

export interface InvestmentGroupInput {
  name: string;
  color: GroupColor;
  allowFractional: boolean;
}

export interface InvestmentHoldingInput {
  groupId: string;
  ticker: string;
  targetPercent: number;
  quantity: number;
  averageCostUsd: number;
  priceArs: number;
}

export interface InvestmentPricesInput {
  usdRate: number;
  prices: { holdingId: string; priceArs: number }[];
}

export const EMPTY_PORTFOLIO: InvestmentPortfolio = {
  targetAmountUsd: 0,
  usdRate: 0,
  updatedAt: null,
  groups: [],
  targetPercent: 0,
  valueUsd: 0,
  costUsd: 0,
  gainUsd: 0,
  gainPercent: 0,
};

export interface InvestmentSnapshot {
  date: string; // yyyy-MM-dd
  usdRate: number;
  valueUsd: number;
  costUsd: number;
  groups: { name: string; color: GroupColor; valueUsd: number; costUsd: number }[];
}

export interface ContributionLine {
  holdingId: string;
  ticker: string;
  units: number;
  amountUsd: number;
  amountArs: number;
  targetPercent: number;
  percentAfter: number;
}

export interface ContributionPlan {
  amountUsd: number;
  usdRate: number;
  investedUsd: number;
  leftoverUsd: number;
  lines: ContributionLine[];
}
