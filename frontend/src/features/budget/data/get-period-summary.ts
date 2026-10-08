import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { BudgetSummaryItem } from "../types/period-summary";
import { session } from "@/src/shared/lib/session";

async function fetchPeriodSummary(token: string, year: number, month: number): Promise<BudgetSummaryItem[]> {
  "use cache";
  cacheTag("budget-summary");
  return fetchApiData<BudgetSummaryItem[]>(`/budget/summary?year=${year}&month=${month}`, token);
}

export async function getPeriodSummary(year: number, month: number): Promise<BudgetSummaryItem[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchPeriodSummary(token, year, month);
}
