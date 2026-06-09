import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { Account } from "../types/account";
import { session } from "@/src/shared/lib/session";

async function fetchAccounts(token: string): Promise<Account[]> {
  "use cache";
  cacheTag("accounts");
  try {
    const response = await serverFetch<ApiResponse<{ accounts: Account[] }>>(
      "/account",
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data.accounts : [];
  } catch {
    return [];
  }
}

export async function getAccounts(): Promise<Account[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchAccounts(token);
}
