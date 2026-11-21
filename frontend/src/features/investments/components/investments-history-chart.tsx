"use client";

import { useMemo, useState } from "react";
import { Area, AreaChart, CartesianGrid, Line, LineChart, ReferenceLine, XAxis, YAxis } from "recharts";
import {
  ChartConfig,
  ChartContainer,
  ChartLegend,
  ChartLegendContent,
  ChartTooltip,
  ChartTooltipContent,
} from "@/src/shared/components/ui/chart";
import { InvestmentSnapshot } from "../types/investment";
import { DisplayCurrency, formatMoney, formatPercent, gainClass, GROUP_COLORS } from "../utils/format";
import { niceScale } from "../utils/chart-scale";

type Range = "3M" | "6M" | "1A" | "ALL";
type Mode = "return" | "value" | "groups";
type Unit = "money" | "percent";

const RANGE_MONTHS: Record<Range, number | null> = { "3M": 3, "6M": 6, "1A": 12, ALL: null };
const RANGE_LABELS: Record<Range, string> = { "3M": "3M", "6M": "6M", "1A": "1A", ALL: "Todo" };
const RANGE_TEXT: Record<Range, string> = {
  "3M": "en los últimos 3 meses",
  "6M": "en los últimos 6 meses",
  "1A": "en el último año",
  ALL: "desde el inicio",
};
const MODE_LABELS: Record<Mode, string> = { return: "Ganancia", value: "Valor vs costo", groups: "Por grupo" };

const POSITIVE = "#16a34a";
const NEGATIVE = "#dc2626";

const VALUE_CONFIG = {
  value: { label: "Valor", color: "#3b82f6" },
  cost: { label: "Costo", color: "#94a3b8" },
} satisfies ChartConfig;
const RETURN_CONFIG = { gain: { label: "Ganancia", color: POSITIVE } } satisfies ChartConfig;

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

// Same as the "Rendimiento total" card: price gain + realized gains + income. The extra
// fields may be missing in data cached before they existed, hence the fallbacks.
function totalReturn(s: InvestmentSnapshot) {
  return s.valueUsd - s.costUsd + (s.realizedGainUsd ?? 0) + (s.incomeUsd ?? 0);
}

// Older snapshots don't know the total invested; their cost is the best estimate.
function invested(s: InvestmentSnapshot) {
  return (s.investedUsd ?? 0) > 0 ? s.investedUsd : s.costUsd;
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
          className={`rounded px-2 py-1 ${value === o ? "bg-primary text-primary-foreground" : "text-muted-foreground"}`}
        >
          {labels[o]}
        </button>
      ))}
    </div>
  );
}

export default function InvestmentsHistoryChart({ history, currency }: InvestmentsHistoryChartProps) {
  const [range, setRange] = useState<Range>("6M");
  const [mode, setMode] = useState<Mode>("return");
  const [unit, setUnit] = useState<Unit>("money");

  const visible = useMemo(() => {
    const months = RANGE_MONTHS[range];
    if (months === null) return history;
    const from = cutoff(months);
    return history.filter((s) => s.date >= from);
  }, [history, range]);

  // ARS uses each day's own rate, so the line shows what it was worth in pesos then.
  const money = (usd: number, rate: number) => (currency === "ARS" ? usd * rate : usd);
  const showPercent = mode === "return" && unit === "percent";
  const fmt = (v: number) => (showPercent ? formatPercent(v) : formatMoney(v, currency));
  const fmtTick = (v: number) => (showPercent ? `${v}%` : formatMoney(v, currency).replace(/,\d+$/, ""));

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
    const gainUsd = totalReturn(s);
    const base = invested(s);
    const row: Record<string, number | string> = {
      date: s.date,
      value: money(s.valueUsd, s.usdRate),
      cost: money(s.costUsd, s.usdRate),
      gain: showPercent ? (base > 0 ? (gainUsd / base) * 100 : 0) : money(gainUsd, s.usdRate),
    };
    let stacked = 0;
    for (const g of groups) {
      const found = s.groups.find((x) => x.name === g.name);
      row[g.key] = found ? money(found.valueUsd, s.usdRate) : 0;
      stacked += Number(row[g.key]);
    }
    row.stacked = stacked;
    return row;
  });

  // Period summary: how the total return moved between the first and last day shown.
  const first = visible[0];
  const last = visible[visible.length - 1];
  const summary =
    first && last
      ? (() => {
          const now = money(totalReturn(last), last.usdRate);
          const change = money(totalReturn(last) - totalReturn(first), last.usdRate);
          const startValue = first.valueUsd;
          const changePct = startValue > 0 ? ((totalReturn(last) - totalReturn(first)) / startValue) * 100 : 0;
          return { now, change, changePct };
        })()
      : null;

  const values =
    mode === "return"
      ? data.map((r) => Number(r.gain)).concat(0)
      : mode === "value"
        ? data.flatMap((r) => [Number(r.value), Number(r.cost)])
        : data.map((r) => Number(r.stacked)).concat(0);
  const scale = niceScale(Math.min(...values), Math.max(...values));

  // The gain area is green above zero and red below. SVG gradients are measured on each
  // shape's own bounding box: the fill spans from the data to zero, the line only the data.
  const gains = data.map((r) => Number(r.gain));
  const zeroOffset = (lo: number, hi: number) => (hi <= 0 ? 0 : lo >= 0 ? 1 : hi / (hi - lo));
  const fillZero = gains.length ? zeroOffset(Math.min(0, ...gains), Math.max(0, ...gains)) : 1;
  const strokeZero = gains.length ? zeroOffset(Math.min(...gains), Math.max(...gains)) : 1;

  const xAxis = (
    <XAxis dataKey="date" tickLine={false} axisLine={false} tickMargin={8} fontSize={11} tickFormatter={formatDay} minTickGap={24} />
  );
  const yAxis = (
    <YAxis
      tickLine={false}
      axisLine={false}
      fontSize={11}
      width={showPercent ? 50 : currency === "ARS" ? 110 : 80}
      domain={scale.domain}
      ticks={scale.ticks}
      tickFormatter={(v) => fmtTick(Number(v))}
    />
  );
  const config: ChartConfig = mode === "return" ? RETURN_CONFIG : mode === "value" ? VALUE_CONFIG : groupConfig;
  const tooltip = (
    <ChartTooltip
      content={
        <ChartTooltipContent
          labelFormatter={(_, payload) => formatDay(String(payload?.[0]?.payload?.date ?? ""))}
          formatter={(value, name, item) => (
            <div className="flex w-full items-center justify-between gap-4">
              <span className="flex items-center gap-1.5 text-muted-foreground">
                <span
                  className="size-2 rounded-[2px]"
                  style={{ background: mode === "return" ? (Number(value) < 0 ? NEGATIVE : POSITIVE) : item.color }}
                />
                {config[String(name)]?.label ?? name}
              </span>
              <span className="font-mono tabular-nums">{fmt(Number(value))}</span>
            </div>
          )}
        />
      }
    />
  );
  const dots = data.length < 2;

  return (
    <section className="flex flex-col gap-3 rounded-lg border p-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex flex-col gap-0.5">
          <h2 className="text-sm font-semibold text-muted-foreground">Evolución</h2>
          {summary && (
            <p className="text-sm">
              <span className={`text-lg font-semibold tabular-nums ${gainClass(summary.change)}`}>
                {`${summary.change >= 0 ? "+" : ""}${formatMoney(summary.change, currency)}`}
              </span>{" "}
              <span className={`tabular-nums ${gainClass(summary.change)}`}>
                {`(${summary.changePct >= 0 ? "+" : ""}${formatPercent(summary.changePct)})`}
              </span>{" "}
              <span className="text-muted-foreground">{`de ganancia ${RANGE_TEXT[range]}`}</span>
            </p>
          )}
        </div>
        <div className="flex flex-wrap gap-2">
          <Toggle label="Vista" value={mode} options={["return", "value", "groups"]} labels={MODE_LABELS} onChange={setMode} />
          {mode === "return" && (
            <Toggle
              label="Unidad"
              value={unit}
              options={["money", "percent"]}
              labels={{ money: currency === "ARS" ? "$" : "US$", percent: "%" }}
              onChange={setUnit}
            />
          )}
          <Toggle label="Rango" value={range} options={["3M", "6M", "1A", "ALL"]} labels={RANGE_LABELS} onChange={setRange} />
        </div>
      </div>

      {visible.length === 0 ? (
        <p className="py-12 text-center text-sm text-muted-foreground">
          {history.length === 0
            ? "El historial arranca con tu próxima actualización: se guarda una foto por día cada vez que cambiás precios o inversiones."
            : "No hay datos en este rango."}
        </p>
      ) : mode === "return" ? (
        <ChartContainer config={RETURN_CONFIG} className="h-[260px] w-full">
          <AreaChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
            <defs>
              <linearGradient id="gain-fill" x1="0" y1="0" x2="0" y2="1">
                <stop offset={0} stopColor={POSITIVE} stopOpacity={0.35} />
                <stop offset={fillZero} stopColor={POSITIVE} stopOpacity={0.05} />
                <stop offset={fillZero} stopColor={NEGATIVE} stopOpacity={0.05} />
                <stop offset={1} stopColor={NEGATIVE} stopOpacity={0.35} />
              </linearGradient>
              <linearGradient id="gain-stroke" x1="0" y1="0" x2="0" y2="1">
                <stop offset={strokeZero} stopColor={POSITIVE} />
                <stop offset={strokeZero} stopColor={NEGATIVE} />
              </linearGradient>
            </defs>
            <CartesianGrid vertical={false} />
            {xAxis}
            {yAxis}
            {tooltip}
            <ReferenceLine y={0} stroke="var(--muted-foreground)" strokeOpacity={0.6} />
            <Area
              dataKey="gain"
              type="monotone"
              stroke="url(#gain-stroke)"
              strokeWidth={2}
              fill="url(#gain-fill)"
              baseValue={0}
              dot={dots}
            />
          </AreaChart>
        </ChartContainer>
      ) : mode === "value" ? (
        <ChartContainer config={VALUE_CONFIG} className="h-[260px] w-full">
          <LineChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
            <CartesianGrid vertical={false} />
            {xAxis}
            {yAxis}
            {tooltip}
            <ChartLegend content={<ChartLegendContent />} />
            <Line dataKey="value" type="monotone" stroke="var(--color-value)" strokeWidth={2} dot={dots} />
            <Line dataKey="cost" type="stepAfter" stroke="var(--color-cost)" strokeWidth={2} strokeDasharray="4 4" dot={dots} />
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
                dot={dots}
              />
            ))}
          </AreaChart>
        </ChartContainer>
      )}
    </section>
  );
}
