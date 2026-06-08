export const dynamic = 'force-dynamic';

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { Entry } from "@/src/features/entries/types/entry";
import { Account } from "@/src/features/accounts/types/account";
import EntriesView from "@/src/features/entries/components/entries-view";

export default async function EntriesPage() {
  let entries: Entry[] = [];
  let accounts: Account[] = [];

  try {
    const [entriesResponse, accountsResponse] = await Promise.all([
      serverFetch<ApiResponse<{ entries: Entry[] }>>("/entry", { auth: true }),
      serverFetch<ApiResponse<{ accounts: Account[] }>>("/account", {
        auth: true,
      }),
    ]);

    if (entriesResponse?.success) {
      entries = entriesResponse.data.entries;
    }
    if (accountsResponse?.success) {
      accounts = accountsResponse.data.accounts;
    }
  } catch (error) {
    console.error("Error fetching entries:", error);
  }

  return <EntriesView entries={entries} accounts={accounts} />;
}
