import { getEntries } from "@/src/features/entries/data/get-entries";
import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import RecentEntries from "./recent-entries";

export default async function RecentEntriesSection() {
  const [entries, accounts] = await Promise.all([getEntries(), getAccounts()]);
  return <RecentEntries entries={entries} accounts={accounts} />;
}
