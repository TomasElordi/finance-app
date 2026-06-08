import { Entry } from "@/src/features/entries/types/entry";
import { Account } from "@/src/features/accounts/types/account";
import CreateEntrySheet from "./create-entry-sheet";
import EntriesList from "./entries-list";
import { AccountProvider } from "../context/account-context";
import EntriesEmptyState from "./entries-empty-state";

interface EntriesViewProps {
  entries: Entry[];
  accounts: Account[];
}

export default function EntriesView({ entries, accounts }: EntriesViewProps) {
  return (
    <AccountProvider accounts={accounts}>
      <div className="p-6 flex flex-col gap-6">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-semibold">Asientos</h1>
          <CreateEntrySheet />
        </div>

        {entries.length === 0 ? (
          <EntriesEmptyState />
        ) : (
          <EntriesList entries={entries} />
        )}
      </div>
    </AccountProvider>
  );
}
