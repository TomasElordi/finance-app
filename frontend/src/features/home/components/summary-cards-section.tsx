import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import SummaryCards from "./summary-cards";

export default async function SummaryCardsSection() {
  const accounts = await getAccounts();
  return <SummaryCards accounts={accounts} />;
}
