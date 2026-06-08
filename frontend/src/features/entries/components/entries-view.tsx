"use client";

import { useMemo } from "react";
import { Entry } from "@/src/features/entries/types/entry";
import { Account } from "@/src/features/accounts/types/account";
import CreateEntrySheet from "./create-entry-sheet";
import EditEntrySheet from "./edit-entry-sheet";
import DeleteEntryForm from "./delete-entry-form";

interface EntriesViewProps {
  entries: Entry[];
  accounts: Account[];
}

export default function EntriesView({ entries, accounts }: EntriesViewProps) {
  const accountsMap = useMemo(
    () => Object.fromEntries(accounts.map((a) => [a.id, a.name])),
    [accounts],
  );

  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Asientos</h1>
        <CreateEntrySheet accounts={accounts} />
      </div>

      {entries.length === 0 ? (
        <p className="text-muted-foreground text-sm">No tenés asientos aún.</p>
      ) : (
        <div className="flex flex-col gap-3">
          {entries
            .sort((a, b) => {
              return b.date.localeCompare(a.date, "es");
            })
            .map((entry) => (
              <div
                key={entry.id}
                className="flex flex-col gap-3 rounded-lg border px-4 py-3"
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="flex flex-col gap-0.5">
                    <span className="font-medium">{entry.title}</span>
                    <span className="text-xs text-muted-foreground">
                      {new Date(entry.date).toLocaleDateString("es-AR", {
                        year: "numeric",
                        month: "short",
                        day: "numeric",
                      })}
                    </span>
                    {entry.description && (
                      <span className="text-sm text-muted-foreground mt-0.5">
                        {entry.description}
                      </span>
                    )}
                  </div>
                  <div className="flex items-center gap-1 shrink-0">
                    <EditEntrySheet entry={entry} accounts={accounts} />
                    <DeleteEntryForm id={entry.id} />
                  </div>
                </div>

                {entry.entryLines.length > 0 && (
                  <div className="flex flex-col gap-1 border-t pt-2">
                    {entry.entryLines.map((line) => (
                      <div
                        key={line.id}
                        className="flex items-center justify-between text-sm"
                      >
                        <span className="text-muted-foreground">
                          {accountsMap[line.accountId] ?? "Cuenta desconocida"}
                        </span>
                        <div className="flex items-center gap-3">
                          <span
                            className={
                              line.type === "Debit"
                                ? "text-red-500"
                                : "text-green-500"
                            }
                          >
                            {line.type === "Debit" ? "D" : "C"} $
                            {Number(line.amount).toFixed(2)}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ))}
        </div>
      )}
    </div>
  );
}
