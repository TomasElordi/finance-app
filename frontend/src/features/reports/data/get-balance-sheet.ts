import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { BalanceSheet } from "../types/balance-sheet";
import { session } from "@/src/shared/lib/session";

function emptyBalanceSheet(year: number): BalanceSheet {
  return { year, assets: [], liabilities: [], equity: [], totalAssets: 0, totalLiabilities: 0, totalEquity: 0 };
}

async function fetchBalanceSheet(token: string, year: number): Promise<BalanceSheet> {
  "use cache";
  cacheTag("reports");
  try {
    const response = await serverFetch<ApiResponse<BalanceSheet>>(
      `/report/balance-sheet?year=${year}`,
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data : emptyBalanceSheet(year);
  } catch {
    return emptyBalanceSheet(year);
  }
}

export async function getBalanceSheet(year: number): Promise<BalanceSheet> {
  const token = await session.getAccessToken();
  if (!token) return emptyBalanceSheet(year);
  return fetchBalanceSheet(token, year);
}
