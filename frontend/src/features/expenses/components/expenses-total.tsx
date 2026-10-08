import { ArrowDownRight, ArrowUpRight } from "lucide-react";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { MonthlyAmount } from "../types/expenses";
import { monthLabel } from "../utils/month-label";

interface ExpensesTotalProps {
  total: number;
  previous?: MonthlyAmount;
}

export default function ExpensesTotal({ total, previous }: ExpensesTotalProps) {
  const diff = previous ? total - previous.amount : 0;
  const pct = previous && previous.amount > 0 ? (diff / previous.amount) * 100 : null;
  const Arrow = diff >= 0 ? ArrowUpRight : ArrowDownRight;

  return (
    <div className="flex flex-col gap-1">
      <span className="text-sm text-muted-foreground">Total gastado</span>
      <span className="text-4xl font-semibold">{formatCurrency(total)}</span>
      {previous && diff !== 0 && (
        <span className="flex items-center gap-1 text-sm text-muted-foreground">
          <Arrow className="size-4" />
          {diff > 0 ? "+" : "−"}
          {formatCurrency(Math.abs(diff))}
          {pct !== null && ` (${diff > 0 ? "+" : "−"}${Math.abs(pct).toFixed(1)}%)`}
          {` vs ${monthLabel(previous.year, previous.month)}`}
        </span>
      )}
    </div>
  );
}
