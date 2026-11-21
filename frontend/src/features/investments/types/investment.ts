export type GroupColor = "amber" | "slate" | "green" | "red" | "blue" | "purple" | "teal" | "pink";

export interface InvestmentHolding {
  id: string;
  groupId: string;
  ticker: string;
  marketSymbol: string | null;
  autoPrice: boolean;
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
  realizedGainUsd: number;
  incomeUsd: number;
  investedUsd: number;
  totalReturnUsd: number;
  totalReturnPercent: number;
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
  realizedGainUsd: number;
  incomeUsd: number;
  investedUsd: number;
  totalReturnUsd: number;
  totalReturnPercent: number;
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
  realizedGainUsd: number;
  incomeUsd: number;
  investedUsd: number;
  totalReturnUsd: number;
  totalReturnPercent: number;
}

export interface InvestmentGroupInput {
  name: string;
  color: GroupColor;
  allowFractional: boolean;
}

export type TradeType = "Buy" | "Sell";
export type IncomeType = "Coupon" | "Dividend" | "Amortization";

export const TRADE_TYPE_LABELS: Record<TradeType, string> = { Buy: "Compra", Sell: "Venta" };
export const INCOME_TYPE_LABELS: Record<IncomeType, string> = {
  Coupon: "Cupón",
  Dividend: "Dividendo",
  Amortization: "Amortización",
};

export interface TradeInput {
  date: string; // yyyy-MM-dd
  type: TradeType;
  quantity: number;
  priceArs: number;
  usdRate: number;
  feeArs: number;
}

export interface InvestmentHoldingInput {
  groupId: string;
  ticker: string;
  marketSymbol: string | null;
  autoPrice: boolean;
  targetPercent: number;
  priceArs: number;
  initialTrade?: TradeInput;
}

export interface IncomeInput {
  holdingId: string;
  date: string;
  type: IncomeType;
  amount: number;
  currency: "USD" | "ARS";
  usdRate: number | null;
}

export interface InvestmentTrade {
  id: string;
  holdingId: string;
  ticker: string;
  date: string;
  type: TradeType;
  quantity: number;
  priceArs: number | null;
  usdRate: number | null;
  priceUsd: number;
  feeArs: number;
  feeUsd: number;
  isInitial: boolean;
}

export interface InvestmentIncome {
  id: string;
  holdingId: string;
  ticker: string;
  date: string;
  type: IncomeType;
  amountUsd: number;
  amountArs: number | null;
  usdRate: number | null;
}

export interface InvestmentActivity {
  trades: InvestmentTrade[];
  incomes: InvestmentIncome[];
}

export interface QuotesResult {
  updated: { ticker: string; symbol: string; priceArs: number }[];
  notFound: { ticker: string; symbol: string; reason: string }[];
  usdRate: number | null;
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
  realizedGainUsd: 0,
  incomeUsd: 0,
  investedUsd: 0,
  totalReturnUsd: 0,
  totalReturnPercent: 0,
};

export interface InvestmentSnapshot {
  date: string; // yyyy-MM-dd
  usdRate: number;
  valueUsd: number;
  costUsd: number;
  // Zero on snapshots taken before these were recorded (investedUsd 0 = unknown).
  realizedGainUsd: number;
  incomeUsd: number;
  investedUsd: number;
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
