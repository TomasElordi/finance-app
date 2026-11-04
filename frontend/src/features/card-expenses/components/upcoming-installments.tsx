import { formatCurrency } from "@/src/features/accounts/utils/format";
import { monthLabel } from "@/src/features/expenses/utils/month-label";
import { CardUpcoming } from "../types/card";

export default function UpcomingInstallments({ upcoming }: { upcoming: CardUpcoming }) {
  if (upcoming.months.length === 0) return null;

  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-muted-foreground">Cuotas pendientes</h2>
      <div className="grid grid-cols-1 gap-3 lg:grid-cols-2">
        <div className="flex flex-col gap-2 rounded-lg border p-4">
          <div className="flex items-baseline justify-between gap-2">
            <span className="font-medium">Total comprometido</span>
            <span className="text-lg font-semibold tabular-nums">{formatCurrency(upcoming.total)}</span>
          </div>
          <dl className="flex flex-col gap-1 text-sm text-muted-foreground">
            {upcoming.people.map((p) => (
              <div key={p.personId ?? "me"} className="flex justify-between gap-2">
                <dt>{p.personName}</dt>
                <dd className="tabular-nums">{formatCurrency(p.total)}</dd>
              </div>
            ))}
          </dl>
        </div>
        <div className="flex flex-col gap-2 rounded-lg border p-4">
          <span className="font-medium">Por mes</span>
          <dl className="flex flex-col gap-1 text-sm text-muted-foreground">
            {upcoming.months.map((m) => (
              <div key={`${m.year}-${m.month}`} className="flex justify-between gap-2">
                <dt className="capitalize">
                  {monthLabel(m.year, m.month)} {m.year}
                </dt>
                <dd className="tabular-nums">{formatCurrency(m.amount)}</dd>
              </div>
            ))}
          </dl>
        </div>
      </div>
    </section>
  );
}
