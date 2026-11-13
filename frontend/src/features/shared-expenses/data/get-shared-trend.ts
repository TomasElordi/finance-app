import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { MonthlyAmount } from "@/src/features/expenses/types/expenses";

async function fetchSharedTrend(
  token: string,
  groupId: string,
  year: number,
  month: number,
): Promise<MonthlyAmount[]> {
  "use cache";
  cacheTag("shared-expenses");
  return fetchApiData<MonthlyAmount[]>(
    `/shared-groups/${groupId}/trend?year=${year}&month=${month}&months=12`,
    token,
  );
}

export async function getSharedTrend(groupId: string, year: number, month: number): Promise<MonthlyAmount[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchSharedTrend(token, groupId, year, month);
}
