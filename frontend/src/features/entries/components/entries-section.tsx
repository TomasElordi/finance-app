import { getEntries } from "@/src/features/entries/data/get-entries";
import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import EntriesView from "./entries-view";

export default async function EntriesSection() {
  const [entries, accounts] = await Promise.all([getEntries(), getAccounts()]);
  return <EntriesView entries={entries} accounts={accounts} />;
}
