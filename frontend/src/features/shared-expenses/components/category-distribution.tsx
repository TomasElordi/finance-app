import { formatCurrency } from "@/src/features/accounts/utils/format";
import { SharedAmount } from "../types/shared";

interface CategoryDistributionProps {
  categories: SharedAmount[];
  total: number;
}

export default function CategoryDistribution({ categories, total }: CategoryDistributionProps) {
  if (categories.length === 0) {
    return <p className="py-4 text-sm text-muted-foreground">Sin gastos en el mes.</p>;
  }

  const sorted = [...categories].sort((a, b) => b.amount - a.amount);
  const max = Math.max(...sorted.map((c) => c.amount), 0);

  return (
    <ul className="flex flex-col gap-1">
      {sorted.map((c) => {
        const share = total > 0 ? (c.amount / total) * 100 : 0;
        const width = max > 0 ? (c.amount / max) * 100 : 0;
        return (
          <li key={c.id ?? "none"} className="flex flex-col gap-1.5 rounded-lg px-3 py-2">
            <div className="flex items-baseline justify-between gap-3 text-sm">
              <span className={c.id ? "truncate" : "truncate text-muted-foreground"}>{c.name}</span>
              <span className="shrink-0 tabular-nums">
                {formatCurrency(c.amount)}
                <span className="ml-2 text-muted-foreground">{share.toFixed(0)}%</span>
              </span>
            </div>
            <div className="h-1.5 w-full rounded-full bg-muted">
              <div className="h-full rounded-full bg-[var(--chart-2)]" style={{ width: `${width}%` }} />
            </div>
          </li>
        );
      })}
    </ul>
  );
}
