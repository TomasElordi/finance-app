import { Entry } from "../types/entry";
import EntryCard from "./entry-card";

interface EntriesListProps {
  entries: Entry[];
}

export default function EntriesList({ entries }: EntriesListProps) {
  const sorted = entries.sort((a, b) => {
    return b.date.localeCompare(a.date, "es");
  });

  return (
    <div className="flex flex-col gap-3">
      {sorted.map((entry) => (
        <EntryCard key={entry.id} entry={entry} />
      ))}
    </div>
  );
}
