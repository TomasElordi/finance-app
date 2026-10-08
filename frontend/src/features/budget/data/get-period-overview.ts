import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { BudgetOverview } from "../types/period-overview";
import { session } from "@/src/shared/lib/session";

const EMPTY_OVERVIEW: BudgetOverview = { totalBudgeted: 0, totalActual: 0, totalIncome: 0 };

async function fetchPeriodOverview(token: string, year: number, month: number): Promise<BudgetOverview> {
  "use cache";
  cacheTag("budget-summary");
  return fetchApiData<BudgetOverview>(`/budget/overview?year=${year}&month=${month}`, token);
}

export async function getPeriodOverview(year: number, month: number): Promise<BudgetOverview> {
  const token = await session.getAccessToken();
  if (!token) return EMPTY_OVERVIEW;
  return fetchPeriodOverview(token, year, month);
}
