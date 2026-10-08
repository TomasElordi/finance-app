import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { IncomeStatement } from "../types/income-statement";
import { session } from "@/src/shared/lib/session";

function emptyIncomeStatement(year: number, month: number): IncomeStatement {
  return { year, month, income: [], expenses: [], totalIncome: 0, totalExpenses: 0, netResult: 0 };
}

async function fetchIncomeStatement(token: string, year: number, month: number): Promise<IncomeStatement> {
  "use cache";
  cacheTag("reports");
  return fetchApiData<IncomeStatement>(`/report/income-statement?year=${year}&month=${month}`, token);
}

export async function getIncomeStatement(year: number, month: number): Promise<IncomeStatement> {
  const token = await session.getAccessToken();
  if (!token) return emptyIncomeStatement(year, month);
  return fetchIncomeStatement(token, year, month);
}
