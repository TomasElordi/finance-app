import Link from "next/link";
import { ReportLine } from "@/src/features/reports/types/report-line";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { cn } from "@/src/shared/lib/utils";
import { expensesHref } from "../utils/month-label";

interface ExpenseDistributionProps {
  lines: ReportLine[];
  total: number;
  year: number;
  month: number;
  selectedAccountId?: string;
}

export default function ExpenseDistribution({
  lines,
  total,
  year,
  month,
  selectedAccountId,
}: ExpenseDistributionProps) {
  if (lines.length === 0) {
    return <p className="text-sm text-muted-foreground py-4">Sin gastos en el período.</p>;
  }

  const sorted = [...lines].sort((a, b) => b.amount - a.amount);
  const max = Math.max(...sorted.map((l) => l.amount), 0);

  return (
    <ul className="flex flex-col gap-1">
      {sorted.map((line) => {
        const selected = line.accountId === selectedAccountId;
        const share = total > 0 ? (line.amount / total) * 100 : 0;
        const width = max > 0 ? Math.max((line.amount / max) * 100, 0) : 0;
        return (
          <li key={line.accountId}>
            <Link
              href={expensesHref(year, month, selected ? undefined : line.accountId)}
              scroll={false}
              aria-current={selected ? "true" : undefined}
              className={cn(
                "flex flex-col gap-1.5 rounded-lg px-3 py-2 transition-colors hover:bg-accent",
                selected && "bg-accent",
              )}
            >
              <div className="flex items-baseline justify-between gap-3 text-sm">
                <span className={cn("truncate", selected && "font-semibold")}>
                  {line.accountName}
                </span>
                <span className="shrink-0 tabular-nums">
                  {formatCurrency(line.amount)}
                  <span className="ml-2 text-muted-foreground">{share.toFixed(0)}%</span>
                </span>
              </div>
              <div className="h-1.5 w-full rounded-full bg-muted">
                <div
                  className={cn(
                    "h-full rounded-full",
                    selected ? "bg-foreground" : "bg-[var(--chart-2)]",
                  )}
                  style={{ width: `${width}%` }}
                />
              </div>
            </Link>
          </li>
        );
      })}
    </ul>
  );
}
