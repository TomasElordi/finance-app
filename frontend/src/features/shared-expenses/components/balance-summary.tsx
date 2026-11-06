import { ArrowRight, CheckCircle2 } from "lucide-react";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { SharedGroupDetail } from "../types/shared";
import SettlementSheet from "./settlement-sheet";

interface BalanceSummaryProps {
  group: SharedGroupDetail;
  year: number;
  month: number;
}

// All-time balance of the group (not just the month being viewed).
export default function BalanceSummary({ group, year, month }: BalanceSummaryProps) {
  const name = (userId: string, fallback: string) =>
    group.members.find((m) => m.userId === userId)?.isMe ? "Vos" : fallback;

  return (
    <section className="flex flex-col gap-3">
      <div className="flex items-center justify-between gap-2">
        <h2 className="text-sm font-semibold text-muted-foreground">Saldo general</h2>
        <SettlementSheet group={group} year={year} month={month} />
      </div>

      <div className="flex flex-col gap-2 rounded-lg border p-4">
        {group.debts.length === 0 ? (
          <p className="flex items-center gap-2 font-medium">
            <CheckCircle2 className="h-5 w-5 text-green-600 dark:text-green-500" />
            Están a mano, nadie le debe a nadie.
          </p>
        ) : (
          group.debts.map((d) => (
            <div key={`${d.fromUserId}-${d.toUserId}`} className="flex flex-wrap items-center justify-between gap-3">
              <p className="flex flex-wrap items-center gap-2">
                <span className="font-medium">{name(d.fromUserId, d.fromName)}</span>
                <span className="text-muted-foreground">
                  {name(d.fromUserId, d.fromName) === "Vos" ? "le debés a" : "le debe a"}
                </span>
                <ArrowRight className="h-4 w-4 text-muted-foreground" />
                <span className="font-medium">{name(d.toUserId, d.toName)}</span>
                <span className="text-lg font-semibold tabular-nums">{formatCurrency(d.amount)}</span>
              </p>
              <SettlementSheet group={group} year={year} month={month} debt={d} />
            </div>
          ))
        )}
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {group.members.map((m) => (
          <div key={m.userId} className="flex flex-col gap-2 rounded-lg border p-4">
            <div className="flex items-baseline justify-between gap-2">
              <span className="font-medium">
                {m.name}
                {m.isMe && <span className="text-muted-foreground"> (vos)</span>}
              </span>
              <span
                className={`text-lg font-semibold tabular-nums ${
                  m.balance > 0
                    ? "text-green-600 dark:text-green-500"
                    : m.balance < 0
                      ? "text-destructive"
                      : ""
                }`}
              >
                {formatCurrency(m.balance)}
              </span>
            </div>
            <dl className="flex flex-col gap-1 text-sm text-muted-foreground">
              <div className="flex justify-between gap-2">
                <dt>Pagó</dt>
                <dd className="tabular-nums">{formatCurrency(m.paid)}</dd>
              </div>
              <div className="flex justify-between gap-2">
                <dt>Le corresponde</dt>
                <dd className="tabular-nums">{formatCurrency(m.share)}</dd>
              </div>
            </dl>
          </div>
        ))}
      </div>
    </section>
  );
}
