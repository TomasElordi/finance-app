import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { BudgetOverview } from "../types/period-overview";
import { session } from "@/src/shared/lib/session";

const EMPTY_OVERVIEW: BudgetOverview = { totalBudgeted: 0, totalActual: 0, totalIncome: 0 };

async function fetchPeriodOverview(token: string, year: number, month: number): Promise<BudgetOverview> {
  "use cache";
  cacheTag("budget-summary");
  try {
    const response = await serverFetch<ApiResponse<BudgetOverview>>(
      `/budget/overview?year=${year}&month=${month}`,
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data : EMPTY_OVERVIEW;
  } catch {
    return EMPTY_OVERVIEW;
  }
}

export async function getPeriodOverview(year: number, month: number): Promise<BudgetOverview> {
  const token = await session.getAccessToken();
  if (!token) return EMPTY_OVERVIEW;
  return fetchPeriodOverview(token, year, month);
}
