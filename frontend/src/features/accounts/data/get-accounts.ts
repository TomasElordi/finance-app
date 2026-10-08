import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { Account } from "../types/account";
import { session } from "@/src/shared/lib/session";

async function fetchAccounts(token: string): Promise<Account[]> {
  "use cache";
  cacheTag("accounts");
  const data = await fetchApiData<{ accounts: Account[] }>("/account", token);
  return data.accounts;
}

export async function getAccounts(): Promise<Account[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchAccounts(token);
}
