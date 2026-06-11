import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { Budget } from "../types/budget";
import { session } from "@/src/shared/lib/session";

async function fetchBudgets(token: string, year: number, month: number): Promise<Budget[]> {
  "use cache";
  cacheTag("budgets");
  try {
    const response = await serverFetch<ApiResponse<Budget[]>>(
      `/budget?year=${year}&month=${month}`,
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data : [];
  } catch {
    return [];
  }
}

export async function getBudgets(year: number, month: number): Promise<Budget[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchBudgets(token, year, month);
}
