import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import AccountsSummary from "./accounts-summary";

export default async function AccountsSummarySection() {
  const accounts = await getAccounts();
  return <AccountsSummary accounts={accounts} />;
}
