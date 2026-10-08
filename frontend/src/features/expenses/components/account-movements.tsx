import { formatCurrency } from "@/src/features/accounts/utils/format";
import { formatEntryDate } from "@/src/features/entries/utils/entry-date";
import { AccountMovement } from "../types/expenses";

export default function AccountMovements({ movements }: { movements: AccountMovement[] }) {
  if (movements.length === 0) {
    return <p className="text-sm text-muted-foreground py-4">Sin movimientos en el período.</p>;
  }

  return (
    <ul className="flex flex-col divide-y rounded-lg border bg-card">
      {movements.map((m, i) => {
        // Debits increase an expense account; credits (refunds, reversals) reduce it
        const isRefund = m.type === "Credit";
        return (
          <li key={`${m.entryId}-${i}`} className="flex items-center justify-between gap-3 px-4 py-3">
            <div className="flex min-w-0 flex-col gap-0.5">
              <span className="truncate font-medium">{m.entryTitle}</span>
              <span className="text-xs text-muted-foreground">
                {formatEntryDate(m.date)}
                {isRefund && " · Reintegro"}
              </span>
            </div>
            <span className="shrink-0 text-sm font-semibold tabular-nums">
              {isRefund ? "−" : ""}
              {formatCurrency(m.amount)}
            </span>
          </li>
        );
      })}
    </ul>
  );
}
