import { formatCurrency } from "@/src/features/accounts/utils/format";
import { SharedPeriod } from "../types/shared";

export default function MonthSummary({ period }: { period: SharedPeriod }) {
  return (
    <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
      <div className="flex flex-col gap-2 rounded-lg border p-4">
        <span className="text-sm font-medium">A cada uno le corresponde</span>
        <dl className="flex flex-col gap-1 text-sm text-muted-foreground">
          {period.shares.map((s) => (
            <Row key={s.id ?? s.name} label={s.name} value={s.amount} />
          ))}
        </dl>
      </div>
      <div className="flex items-baseline justify-between rounded-lg bg-muted px-4 py-3">
        <span className="font-medium">Total del mes</span>
        <span className="text-2xl font-semibold tabular-nums">{formatCurrency(period.total)}</span>
      </div>
    </div>
  );
}

function Row({ label, value }: { label: string; value: number }) {
  return (
    <div className="flex justify-between gap-2">
      <dt>{label}</dt>
      <dd className="tabular-nums">{formatCurrency(value)}</dd>
    </div>
  );
}
