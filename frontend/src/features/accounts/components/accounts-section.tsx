import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import AccountsView from "./accounts-view";

export default async function AccountsSection() {
  const accounts = await getAccounts();
  return <AccountsView accounts={accounts} />;
}
