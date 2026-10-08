import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { AccountMovement } from "../types/expenses";
import { session } from "@/src/shared/lib/session";

async function fetchAccountMovements(
  token: string,
  accountId: string,
  year: number,
  month: number,
): Promise<AccountMovement[]> {
  "use cache";
  cacheTag("reports");
  return fetchApiData<AccountMovement[]>(
    `/report/account-movements?accountId=${encodeURIComponent(accountId)}&year=${year}&month=${month}`,
    token,
  );
}

export async function getAccountMovements(
  accountId: string,
  year: number,
  month: number,
): Promise<AccountMovement[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchAccountMovements(token, accountId, year, month);
}
