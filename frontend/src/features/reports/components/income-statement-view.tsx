import { IncomeStatement } from "../types/income-statement";
import { Card, CardContent, CardHeader, CardTitle } from "@/src/shared/components/ui/card";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { cn } from "@/src/shared/lib/utils";
import { Scale, TrendingDown, TrendingUp } from "lucide-react";
import ReportLineList from "./report-line-list";

export default function IncomeStatementView({
  income,
  expenses,
  totalIncome,
  totalExpenses,
  netResult,
}: IncomeStatement) {
  const isProfit = netResult >= 0;

  const cards = [
    { title: "Total Ingresos", value: totalIncome, icon: TrendingUp, colorClass: "text-emerald-500" },
    { title: "Total Gastos", value: totalExpenses, icon: TrendingDown, colorClass: "text-red-500" },
    {
      title: "Resultado Neto",
      value: netResult,
      icon: Scale,
      colorClass: isProfit ? "text-emerald-500" : "text-red-500",
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
        {cards.map((card) => (
          <Card key={card.title}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">{card.title}</CardTitle>
              <card.icon className={cn("size-4", card.colorClass)} />
            </CardHeader>
            <CardContent>
              <span className={cn("text-xl font-bold", card.title === "Resultado Neto" && card.colorClass)}>
                {formatCurrency(card.value)}
              </span>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-6 md:grid-cols-2">
        <ReportLineList title="Ingresos" lines={income} emptyLabel="Sin ingresos en el período" />
        <ReportLineList title="Gastos" lines={expenses} emptyLabel="Sin gastos en el período" />
      </div>
    </div>
  );
}
