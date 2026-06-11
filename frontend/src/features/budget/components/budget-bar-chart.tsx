"use client";

import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
} from "@/src/shared/components/ui/chart";
import { BudgetSummaryItem } from "../types/period-summary";
import { formatCurrency } from "@/src/features/accounts/utils/format";

const chartConfig = {
  budgetedAmount: {
    label: "Presupuestado",
    color: "var(--chart-1)",
  },
  actualAmount: {
    label: "Real",
    color: "var(--chart-2)",
  },
};

export default function BudgetBarChart({ items }: { items: BudgetSummaryItem[] }) {
  if (items.length === 0) return null;

  const data = items.map((item) => ({
    name: item.accountName,
    budgetedAmount: item.budgetedAmount,
    actualAmount: item.actualAmount,
  }));

  return (
    <ChartContainer config={chartConfig} className="min-h-[300px] w-full">
      <BarChart data={data} margin={{ top: 8, right: 8, bottom: 8, left: 8 }}>
        <CartesianGrid vertical={false} />
        <XAxis
          dataKey="name"
          tickLine={false}
          axisLine={false}
          tickMargin={8}
          fontSize={12}
        />
        <YAxis
          tickLine={false}
          axisLine={false}
          tickFormatter={(v) => formatCurrency(v)}
          fontSize={11}
          width={90}
        />
        <ChartTooltip
          content={
            <ChartTooltipContent
              formatter={(value) => formatCurrency(Number(value))}
            />
          }
        />
        <ChartLegend content={<ChartLegendContent />} />
        <Bar dataKey="budgetedAmount" fill="var(--color-budgetedAmount)" radius={4} />
        <Bar dataKey="actualAmount" fill="var(--color-actualAmount)" radius={4} />
      </BarChart>
    </ChartContainer>
  );
}
