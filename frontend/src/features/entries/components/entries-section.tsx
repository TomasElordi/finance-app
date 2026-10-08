import { getEntries } from "@/src/features/entries/data/get-entries";
import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import EntriesView from "./entries-view";

interface EntriesSectionProps {
  page: number;
}

export default async function EntriesSection({ page }: EntriesSectionProps) {
  const [entries, accounts] = await Promise.all([getEntries(page), getAccounts()]);
  return <EntriesView entries={entries} accounts={accounts} />;
}
