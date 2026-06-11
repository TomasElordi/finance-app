import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { BudgetSummaryItem } from "../types/period-summary";
import { session } from "@/src/shared/lib/session";

async function fetchPeriodSummary(token: string, year: number, month: number): Promise<BudgetSummaryItem[]> {
  "use cache";
  cacheTag("budget-summary");
  try {
    const response = await serverFetch<ApiResponse<BudgetSummaryItem[]>>(
      `/budget/summary?year=${year}&month=${month}`,
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data : [];
  } catch {
    return [];
  }
}

export async function getPeriodSummary(year: number, month: number): Promise<BudgetSummaryItem[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchPeriodSummary(token, year, month);
}
