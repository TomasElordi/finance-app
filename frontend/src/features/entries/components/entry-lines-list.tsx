import { EntryLine } from "../types/entry";
import EntryLineDetail from "./entry-line-detail";

export default function EntryLinesList({
  entryLines,
}: {
  entryLines: EntryLine[];
}) {
  return (
    <div className="flex flex-col gap-1 border-t pt-2">
      {entryLines.map((line) => (
        <EntryLineDetail key={line.id} entryLine={line} />
      ))}
    </div>
  );
}
