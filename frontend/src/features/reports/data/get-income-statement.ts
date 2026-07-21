import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { IncomeStatement } from "../types/income-statement";
import { session } from "@/src/shared/lib/session";

function emptyIncomeStatement(year: number, month: number): IncomeStatement {
  return { year, month, income: [], expenses: [], totalIncome: 0, totalExpenses: 0, netResult: 0 };
}

async function fetchIncomeStatement(token: string, year: number, month: number): Promise<IncomeStatement> {
  "use cache";
  cacheTag("reports");
  try {
    const response = await serverFetch<ApiResponse<IncomeStatement>>(
      `/report/income-statement?year=${year}&month=${month}`,
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data : emptyIncomeStatement(year, month);
  } catch {
    return emptyIncomeStatement(year, month);
  }
}

export async function getIncomeStatement(year: number, month: number): Promise<IncomeStatement> {
  const token = await session.getAccessToken();
  if (!token) return emptyIncomeStatement(year, month);
  return fetchIncomeStatement(token, year, month);
}
