import { formatCurrency } from "@/src/features/accounts/utils/format";
import { CardPersonTotal } from "../types/card";

interface PersonTotalsProps {
  people: CardPersonTotal[];
  total: number;
}

export default function PersonTotals({ people, total }: PersonTotalsProps) {
  return (
    <div className="flex flex-col gap-3">
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {people.map((p) => (
          <div key={p.personId ?? "me"} className="flex flex-col gap-2 rounded-lg border p-4">
            <div className="flex items-baseline justify-between gap-2">
              <span className="font-medium">{p.personName}</span>
              <span className="text-lg font-semibold tabular-nums">{formatCurrency(p.total)}</span>
            </div>
            <dl className="flex flex-col gap-1 text-sm text-muted-foreground">
              <Row label="Gastos" value={p.expenses} />
              {p.taxes !== 0 && <Row label="Impuestos" value={p.taxes} />}
              {p.credits !== 0 && <Row label="Saldos a favor" value={-p.credits} />}
            </dl>
          </div>
        ))}
      </div>
      <div className="flex items-baseline justify-between rounded-lg bg-muted px-4 py-3">
        <span className="font-medium">Total del resumen</span>
        <span className="text-2xl font-semibold tabular-nums">{formatCurrency(total)}</span>
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
