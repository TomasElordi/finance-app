"use client";

import { BudgetSummaryItem } from "../types/period-summary";
import { Card, CardContent, CardHeader, CardTitle } from "@/src/shared/components/ui/card";
import { formatCurrency } from "@/src/features/accounts/utils/format";

function getStatusColor(percentage: number): string {
  if (percentage >= 100) return "bg-red-500";
  if (percentage >= 80) return "bg-yellow-500";
  return "bg-green-500";
}

function getStatusTextColor(percentage: number): string {
  if (percentage >= 100) return "text-red-600";
  if (percentage >= 80) return "text-yellow-600";
  return "text-green-600";
}

export default function BudgetProgressCards({ items }: { items: BudgetSummaryItem[] }) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground text-center py-8">
        No hay presupuestos definidos para este período.
      </p>
    );
  }

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {items.map((item) => (
        <Card key={item.accountId}>
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">{item.accountName}</CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-3">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Gastado</span>
              <span className="font-semibold tabular-nums">{formatCurrency(item.actualAmount)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Presupuestado</span>
              <span className="tabular-nums">{formatCurrency(item.budgetedAmount)}</span>
            </div>
            <div className="flex flex-col gap-1">
              <div className="h-2 w-full rounded-full bg-muted overflow-hidden">
                <div
                  className={`h-full rounded-full transition-all ${getStatusColor(item.percentage)}`}
                  style={{ width: `${Math.min(item.percentage, 100)}%` }}
                />
              </div>
              <span className={`text-xs font-semibold self-end ${getStatusTextColor(item.percentage)}`}>
                {item.percentage.toFixed(1)}%
              </span>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
