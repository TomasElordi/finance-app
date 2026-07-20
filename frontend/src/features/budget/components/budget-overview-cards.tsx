import { BudgetOverview } from "../types/period-overview";
import { Card, CardContent, CardHeader, CardTitle } from "@/src/shared/components/ui/card";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { cn } from "@/src/shared/lib/utils";
import { Target, TrendingDown, TrendingUp } from "lucide-react";

export default function BudgetOverviewCards({ totalBudgeted, totalActual, totalIncome }: BudgetOverview) {
  const remaining = totalBudgeted - totalActual;
  const overBudget = totalActual > totalBudgeted;

  const cards = [
    {
      title: "Presupuestado",
      value: totalBudgeted,
      icon: Target,
      colorClass: "text-blue-500",
    },
    {
      title: "Gastado",
      value: totalActual,
      icon: overBudget ? TrendingUp : TrendingDown,
      colorClass: overBudget ? "text-red-500" : "text-green-500",
    },
    {
      title: "Ingresos del período",
      value: totalIncome,
      icon: TrendingUp,
      colorClass: "text-emerald-500",
    },
  ];

  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
      {cards.map((card) => (
        <Card key={card.title}>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">{card.title}</CardTitle>
            <card.icon className={cn("size-4", card.colorClass)} />
          </CardHeader>
          <CardContent>
            <span className="text-xl font-bold">{formatCurrency(card.value)}</span>
            {card.title === "Gastado" && (
              <p className={cn("text-xs mt-1", overBudget ? "text-red-500" : "text-muted-foreground")}>
                {overBudget
                  ? `${formatCurrency(Math.abs(remaining))} por encima del presupuesto`
                  : `${formatCurrency(remaining)} disponible`}
              </p>
            )}
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
