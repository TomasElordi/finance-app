import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { Budget } from "../types/budget";
import { session } from "@/src/shared/lib/session";

async function fetchBudgets(token: string, year: number, month: number): Promise<Budget[]> {
  "use cache";
  cacheTag("budgets");
  return fetchApiData<Budget[]>(`/budget?year=${year}&month=${month}`, token);
}

export async function getBudgets(year: number, month: number): Promise<Budget[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchBudgets(token, year, month);
}
