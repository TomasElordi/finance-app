export const dynamic = 'force-dynamic';

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { Account } from "@/src/features/accounts/types/account";
import AccountsView from "@/src/features/accounts/components/accounts-view";

export default async function AccountsPage() {
  let accounts: Account[] = [];

  try {
    const response = await serverFetch<ApiResponse<{ accounts: Account[] }>>(
      "/account",
      { auth: true },
    );
    if (response?.success) {
      accounts = response.data.accounts;
    }
  } catch (error) {
    console.error("Error fetching accounts:", error);
  }

  return <AccountsView accounts={accounts} />;
}
