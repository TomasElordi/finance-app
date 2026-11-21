"use client";

import { useMemo, useState } from "react";
import { Area, AreaChart, CartesianGrid, Line, LineChart, XAxis, YAxis } from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "@/src/shared/components/ui/chart";
import { InvestmentSnapshot } from "../types/investment";
import { DisplayCurrency, formatMoney, GROUP_COLORS } from "../utils/format";

type Range = "3M" | "6M" | "1A" | "ALL";
type Mode = "total" | "groups";

const RANGE_MONTHS: Record<Range, number | null> = { "3M": 3, "6M": 6, "1A": 12, ALL: null };
const RANGE_LABELS: Record<Range, string> = { "3M": "3M", "6M": "6M", "1A": "1A", ALL: "Todo" };

const TOTAL_CONFIG = {
  value: { label: "Valor", color: "#3b82f6" },
  cost: { label: "Costo", color: "#94a3b8" },
} satisfies ChartConfig;

interface InvestmentsHistoryChartProps {
  history: InvestmentSnapshot[];
  currency: DisplayCurrency;
}

function formatDay(date: string) {
  const [y, m, d] = date.split("-");
  return `${d}/${m}/${y.slice(2)}`;
}

function cutoff(months: number) {
  const d = new Date();
  d.setMonth(d.getMonth() - months);
  return d.toISOString().slice(0, 10);
}

function Toggle<T extends string>({
  value,
  options,
  labels,
  onChange,
  label,
}: {
  value: T;
  options: T[];
  labels: Record<T, string>;
  onChange: (v: T) => void;
  label: string;
}) {
  return (
    <div className="flex rounded-md border p-0.5 text-xs" role="group" aria-label={label}>
      {options.map((o) => (
        <button
          key={o}
          type="button"
          aria-pressed={value === o}
          onClick={() => onChange(o)}
          className={`rounded px-2 py-1 ${
            value === o ? "bg-primary text-primary-foreground" : "text-muted-foreground"
          }`}
        >
          {labels[o]}
        </button>
      ))}
    </div>
  );
}

export default function InvestmentsHistoryChart({ history, currency }: InvestmentsHistoryChartProps) {
  const [range, setRange] = useState<Range>("6M");
  const [mode, setMode] = useState<Mode>("total");

  const visible = useMemo(() => {
    const months = RANGE_MONTHS[range];
    if (months === null) return history;
    const from = cutoff(months);
    return history.filter((s) => s.date >= from);
  }, [history, range]);

  // ARS uses each day's own rate, so the line shows what the portfolio was worth in pesos then.
  const money = (usd: number, rate: number) => (currency === "ARS" ? usd * rate : usd);

  // Group names can repeat colors or contain spaces, so each gets a positional key.
  const groups = useMemo(() => {
    const byName = new Map<string, string>();
    for (const s of visible) for (const g of s.groups) byName.set(g.name, g.color);
    return [...byName.entries()].map(([name, color], i) => ({
      key: `g${i}`,
      name,
      color: GROUP_COLORS[color as keyof typeof GROUP_COLORS]?.chart ?? "var(--chart-3)",
    }));
  }, [visible]);

  const groupConfig = useMemo(
    () => Object.fromEntries(groups.map((g) => [g.key, { label: g.name, color: g.color }])) as ChartConfig,
    [groups],
  );

  const data = visible.map((s) => {
    const row: Record<string, number | string> = {
      date: s.date,
      value: money(s.valueUsd, s.usdRate),
      cost: money(s.costUsd, s.usdRate),
    };
    for (const g of groups) {
      const found = s.groups.find((x) => x.name === g.name);
      row[g.key] = found ? money(found.valueUsd, s.usdRate) : 0;
    }
    return row;
  });

  const totals = data.flatMap((r) => [Number(r.value), Number(r.cost)]);
  const totalDomain: [number, number] = totals.length
    ? [Math.floor(Math.min(...totals) * 0.97), Math.ceil(Math.max(...totals) * 1.02)]
    : [0, 1];

  const yAxis = (
    <YAxis
      tickLine={false}
      axisLine={false}
      fontSize={11}
      width={currency === "ARS" ? 110 : 80}
      // Stacked groups start at zero; the total zooms in so day-to-day moves are visible.
      domain={mode === "total" ? totalDomain : [0, "auto"]}
      tickFormatter={(v) => formatMoney(Number(v), currency).replace(/,\d+$/, "")}
    />
  );
  const xAxis = (
    <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} fontSize={11} tickFormatter={formatDay} />
  );
  const tooltip = (
    <ChartTooltip
      content={
        <ChartTooltipContent
          labelFormatter={(_, payload) => formatDay(String(payload?.[0]?.payload?.date ?? ""))}
          formatter={(value, name, item) => (
            <div className="flex w-full items-center justify-between gap-4">
              <span className="flex items-center gap-1.5 text-muted-foreground">
                <span className="size-2 rounded-[2px]" style={{ background: item.color }} />
                {((mode === "total" ? TOTAL_CONFIG : groupConfig) as ChartConfig)[String(name)]?.label ?? name}
              </span>
              <span className="font-mono tabular-nums">{formatMoney(Number(value), currency)}</span>
            </div>
          )}
        />
      }
    />
  );

  return (
    <section className="flex flex-col gap-3 rounded-lg border p-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-sm font-semibold text-muted-foreground">Evolución</h2>
        <div className="flex flex-wrap gap-2">
          <Toggle
            label="Vista"
            value={mode}
            options={["total", "groups"]}
            labels={{ total: "Total", groups: "Por grupo" }}
            onChange={setMode}
          />
          <Toggle
            label="Rango"
            value={range}
            options={["3M", "6M", "1A", "ALL"]}
            labels={RANGE_LABELS}
            onChange={setRange}
          />
        </div>
      </div>

      {visible.length === 0 ? (
        <p className="py-12 text-center text-sm text-muted-foreground">
          {history.length === 0
            ? "El historial arranca con tu próxima actualización: se guarda una foto por día cada vez que cambiás precios o inversiones."
            : "No hay datos en este rango."}
        </p>
      ) : mode === "total" ? (
        <ChartContainer config={TOTAL_CONFIG} className="h-[260px] w-full">
          <LineChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
            <CartesianGrid vertical={false} />
            {xAxis}
            {yAxis}
            {tooltip}
            <ChartLegend content={<ChartLegendContent />} />
            <Line dataKey="value" type="monotone" stroke="var(--color-value)" strokeWidth={2} dot={data.length < 2} />
            <Line
              dataKey="cost"
              type="stepAfter"
              stroke="var(--color-cost)"
              strokeWidth={2}
              strokeDasharray="4 4"
              dot={data.length < 2}
            />
          </LineChart>
        </ChartContainer>
      ) : (
        <ChartContainer config={groupConfig} className="h-[260px] w-full">
          <AreaChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
            <CartesianGrid vertical={false} />
            {xAxis}
            {yAxis}
            {tooltip}
            <ChartLegend content={<ChartLegendContent />} />
            {groups.map((g) => (
              <Area
                key={g.key}
                dataKey={g.key}
                type="monotone"
                stackId="groups"
                stroke={`var(--color-${g.key})`}
                fill={`var(--color-${g.key})`}
                fillOpacity={0.35}
                dot={data.length < 2}
              />
            ))}
          </AreaChart>
        </ChartContainer>
      )}
    </section>
  );
}
