import { Entry } from "@/src/features/entries/types/entry";
import { Account } from "@/src/features/accounts/types/account";
import EntryCard from "@/src/features/entries/components/entry-card";
import { AccountProvider } from "@/src/features/entries/context/account-context";

interface RecentEntriesProps {
  entries: Entry[];
  accounts: Account[];
}

export default function RecentEntries({ entries, accounts }: RecentEntriesProps) {
  const recent = [...entries]
    .sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
    .slice(0, 5);

  return (
    <div className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Últimos asientos
      </h2>
      {recent.length === 0 ? (
        <p className="text-sm text-muted-foreground">No hay asientos aún.</p>
      ) : (
        <AccountProvider accounts={accounts}>
          <div className="flex flex-col gap-2">
            {recent.map((entry) => (
              <EntryCard key={entry.id} entry={entry} />
            ))}
          </div>
        </AccountProvider>
      )}
    </div>
  );
}
