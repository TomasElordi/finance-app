"use client";

import { useRouter } from "next/navigation";
import { Bar, BarChart, CartesianGrid, Cell, LabelList, XAxis, YAxis } from "recharts";
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/src/shared/components/ui/chart";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { MonthlyAmount } from "../types/expenses";
import { expensesHref, monthLabel } from "../utils/month-label";

const chartConfig = {
  amount: { label: "Gastado", color: "var(--chart-2)" },
};

const compactCurrency = new Intl.NumberFormat("es-AR", {
  style: "currency",
  currency: "ARS",
  notation: "compact",
  maximumFractionDigits: 1,
});

interface ExpenseTrendChartProps {
  data: MonthlyAmount[];
  year: number;
  month: number;
  accountId?: string;
}

export default function ExpenseTrendChart({ data, year, month, accountId }: ExpenseTrendChartProps) {
  const router = useRouter();

  const rows = data.map((d) => ({
    ...d,
    label: monthLabel(d.year, d.month, "short"),
    fullLabel: `${monthLabel(d.year, d.month)} ${d.year}`,
    selected: d.year === year && d.month === month,
  }));

  return (
    <ChartContainer config={chartConfig} className="h-[280px] w-full">
      <BarChart data={rows} margin={{ top: 24, right: 24, bottom: 0, left: 4 }} barCategoryGap={2}>
        <CartesianGrid vertical={false} />
        <XAxis dataKey="label" tickLine={false} axisLine={false} tickMargin={8} fontSize={12} />
        <YAxis
          tickLine={false}
          axisLine={false}
          tickFormatter={(v) => compactCurrency.format(v)}
          fontSize={11}
          width={64}
        />
        <ChartTooltip
          cursor={{ fill: "var(--muted)" }}
          content={
            <ChartTooltipContent
              labelFormatter={(_, payload) => payload?.[0]?.payload?.fullLabel}
              formatter={(value) => formatCurrency(Number(value))}
            />
          }
        />
        <Bar
          dataKey="amount"
          radius={[4, 4, 0, 0]}
          className="cursor-pointer"
          onClick={(_, index) => {
            const row = rows[index];
            router.push(expensesHref(row.year, row.month, accountId), { scroll: false });
          }}
        >
          {rows.map((row) => (
            <Cell
              key={`${row.year}-${row.month}`}
              fill={row.selected ? "var(--foreground)" : "var(--color-amount)"}
            />
          ))}
          <LabelList
            dataKey="amount"
            // Direct-label only the selected month; the tooltip carries the rest
            content={({ x, y, width, index }) => {
              if (index === undefined || !rows[index]?.selected) return null;
              return (
                <text
                  x={Number(x) + Number(width) / 2}
                  y={Number(y) - 8}
                  textAnchor="middle"
                  fontSize={11}
                  className="fill-foreground"
                >
                  {compactCurrency.format(rows[index].amount)}
                </text>
              );
            }}
          />
        </Bar>
      </BarChart>
    </ChartContainer>
  );
}
