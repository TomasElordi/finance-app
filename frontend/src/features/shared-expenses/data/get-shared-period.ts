import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { SharedPeriod } from "../types/shared";

async function fetchSharedPeriod(
  token: string,
  groupId: string,
  year: number,
  month: number,
): Promise<SharedPeriod> {
  "use cache";
  cacheTag("shared-expenses");
  return fetchApiData<SharedPeriod>(
    `/shared-groups/${groupId}/period?year=${year}&month=${month}`,
    token,
  );
}

export async function getSharedPeriod(groupId: string, year: number, month: number): Promise<SharedPeriod> {
  const token = await session.getAccessToken();
  if (!token) return { year, month, expenses: [], settlements: [], total: 0, shares: [], byCategory: [] };
  return fetchSharedPeriod(token, groupId, year, month);
}
