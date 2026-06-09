import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import AccountsView from "@/src/features/accounts/components/accounts-view";

export default async function AccountsPage() {
  const accounts = await getAccounts();
  return <AccountsView accounts={accounts} />;
}
