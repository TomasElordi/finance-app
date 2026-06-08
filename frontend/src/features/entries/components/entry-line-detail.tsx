"use client";
import { useAccounts } from "../context/account-context";
import { EntryLine } from "../types/entry";

export default function EntryLineDetail({
  entryLine,
}: {
  entryLine: EntryLine;
}) {
  const accountContext = useAccounts();
  const accountsMap = accountContext.accountsMap;
  const accountName = accountsMap[entryLine.accountId] ?? "Cuenta desconocida";
  const typeText = entryLine.type === "Debit" ? "D" : "C";
  const amount = Number(entryLine.amount).toFixed(2);
  return (
    <div
      key={entryLine.id}
      className="flex items-center justify-between text-sm"
    >
      <span className="text-muted-foreground">{accountName}</span>
      <div className="flex items-center gap-3">
        <span
          className={
            entryLine.type === "Debit" ? "text-red-500" : "text-green-500"
          }
        >
          {`${typeText} $${amount}`}
        </span>
      </div>
    </div>
  );
}
