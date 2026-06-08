import { Entry } from "../types/entry";
import DeleteEntryForm from "./delete-entry-form";
import EditEntrySheet from "./edit-entry-sheet";
import EntryLinesList from "./entry-lines-list";

export default function EntryCard({ entry }: { entry: Entry }) {
  const date = new Date(entry.date).toLocaleDateString("es-AR", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
  return (
    <div
      key={entry.id}
      className="flex flex-col gap-3 rounded-lg border px-4 py-3"
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-col gap-0.5">
          <span className="font-medium">{entry.title}</span>
          <span className="text-xs text-muted-foreground">{date}</span>
          {entry.description && (
            <span className="text-sm text-muted-foreground mt-0.5">
              {entry.description}
            </span>
          )}
        </div>
        <div className="flex items-center gap-1 shrink-0">
          <EditEntrySheet entry={entry} />
          <DeleteEntryForm id={entry.id} />
        </div>
      </div>

      {entry.entryLines.length > 0 && (
        <EntryLinesList entryLines={entry.entryLines} />
      )}
    </div>
  );
}
