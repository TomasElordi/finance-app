import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { MonthlyAmount } from "../types/expenses";
import { session } from "@/src/shared/lib/session";

async function fetchExpenseTrend(
  token: string,
  year: number,
  month: number,
  accountId?: string,
): Promise<MonthlyAmount[]> {
  "use cache";
  cacheTag("reports");
  const account = accountId ? `&accountId=${encodeURIComponent(accountId)}` : "";
  return fetchApiData<MonthlyAmount[]>(
    `/report/expense-trend?year=${year}&month=${month}&months=12${account}`,
    token,
  );
}

export async function getExpenseTrend(
  year: number,
  month: number,
  accountId?: string,
): Promise<MonthlyAmount[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchExpenseTrend(token, year, month, accountId);
}
