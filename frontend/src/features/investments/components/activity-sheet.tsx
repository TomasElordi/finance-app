"use client";

import { useState, useTransition } from "react";
import { History, Trash2 } from "lucide-react";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/src/shared/components/ui/sheet";
import { Button } from "@/src/shared/components/ui/button";
import { Input } from "@/src/shared/components/ui/input";
import { Field, FieldLabel } from "@/src/shared/components/ui/field";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import {
  INCOME_TYPE_LABELS,
  IncomeType,
  InvestmentActivity,
  InvestmentHolding,
  TRADE_TYPE_LABELS,
  TradeType,
} from "../types/investment";
import { formatDate, formatMoney, formatQuantity, gainClass, todayInArgentina } from "../utils/format";
import {
  addIncomeAction,
  addTradeAction,
  deleteIncomeAction,
  deleteTradeAction,
  getActivityAction,
} from "../actions/investment-actions";

type Tab = "trade" | "income";

interface ActivitySheetProps {
  holding: InvestmentHolding;
  usdRate: number;
}

// A holding's history: buys and sells (which drive quantity and PPC) and the cash it paid.
export default function ActivitySheet({ holding, usdRate }: ActivitySheetProps) {
  const [open, setOpen] = useState(false);
  const [activity, setActivity] = useState<InvestmentActivity | null>(null);
  const [tab, setTab] = useState<Tab>("trade");
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const [trade, setTrade] = useState({ type: "Buy" as TradeType, date: "", quantity: "", priceArs: "", usdRate: "", feeArs: "" });
  const [income, setIncome] = useState({ type: "Coupon" as IncomeType, date: "", amount: "", currency: "USD" as "USD" | "ARS", usdRate: "" });

  function resetForms() {
    const rate = usdRate ? String(usdRate) : "";
    const today = todayInArgentina();
    setTrade({ type: "Buy", date: today, quantity: "", priceArs: String(holding.priceArs || ""), usdRate: rate, feeArs: "" });
    setIncome({ type: "Coupon", date: today, amount: "", currency: "USD", usdRate: rate });
  }

  function load() {
    startTransition(async () => {
      const result = await getActivityAction(holding.id);
      if (!result) setError("No se pudo cargar el historial.");
      setActivity(result);
    });
  }

  function handleOpenChange(next: boolean) {
    if (next) {
      setError(null);
      setActivity(null);
      resetForms();
      load();
    }
    setOpen(next);
  }

  function run(action: () => Promise<{ status: string; message?: string }>, after?: () => void) {
    setError(null);
    startTransition(async () => {
      const result = await action();
      if (result.status === "error") {
        setError(result.message ?? "Error");
        return;
      }
      after?.();
      const fresh = await getActivityAction(holding.id);
      setActivity(fresh);
    });
  }

  function handleAddTrade() {
    const quantity = Number(trade.quantity);
    const priceArs = Number(trade.priceArs);
    const rate = Number(trade.usdRate);
    const fee = Number(trade.feeArs || 0);
    if (!(quantity > 0)) return setError("La cantidad debe ser mayor a cero.");
    if (!(priceArs >= 0)) return setError("Revisá el precio.");
    if (!(rate > 0)) return setError("Ingresá el dólar de ese día.");
    if (!(fee >= 0)) return setError("La comisión no puede ser negativa.");
    run(
      () => addTradeAction(holding.id, { type: trade.type, date: trade.date, quantity, priceArs, usdRate: rate, feeArs: fee }),
      resetForms,
    );
  }

  function handleAddIncome() {
    const amount = Number(income.amount);
    const rate = Number(income.usdRate);
    if (!(amount > 0)) return setError("El monto debe ser mayor a cero.");
    if (income.currency === "ARS" && !(rate > 0)) return setError("Ingresá el dólar de ese día.");
    run(
      () =>
        addIncomeAction({
          holdingId: holding.id,
          type: income.type,
          date: income.date,
          amount,
          currency: income.currency,
          usdRate: income.currency === "ARS" ? rate : null,
        }),
      resetForms,
    );
  }

  const tabButton = (value: Tab, label: string) => (
    <button
      type="button"
      aria-pressed={tab === value}
      onClick={() => {
        setTab(value);
        setError(null);
      }}
      className={`flex-1 rounded px-3 py-1 ${tab === value ? "bg-primary text-primary-foreground" : "text-muted-foreground"}`}
    >
      {label}
    </button>
  );

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon-sm" aria-label={`Operaciones y cobros de ${holding.ticker}`} title="Operaciones y cobros">
          <History className="h-4 w-4" />
        </Button>
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-2xl">
        <SheetHeader>
          <SheetTitle>{holding.ticker}</SheetTitle>
          <SheetDescription>Compras, ventas y cobros. La cantidad y el PPC se recalculan solos.</SheetDescription>
        </SheetHeader>

        <div className="flex flex-col gap-5 p-4">
          <dl className="grid grid-cols-2 gap-3 text-sm sm:grid-cols-4">
            <Stat label="Cantidad" value={formatQuantity(holding.quantity)} />
            <Stat label="PPC" value={formatMoney(holding.averageCostUsd, "USD")} />
            <Stat label="Ganancia realizada" value={formatMoney(holding.realizedGainUsd, "USD")} valueClass={gainClass(holding.realizedGainUsd)} />
            <Stat label="Cobrado" value={formatMoney(holding.incomeUsd, "USD")} />
          </dl>

          <div className="flex flex-col gap-3 rounded-lg border p-3">
            <div className="flex rounded-md border p-0.5 text-sm" role="group" aria-label="Qué registrar">
              {tabButton("trade", "Compra / venta")}
              {tabButton("income", "Cobro")}
            </div>

            {tab === "trade" ? (
              <div className="grid grid-cols-2 gap-3">
                <Field>
                  <FieldLabel>Tipo</FieldLabel>
                  <Select value={trade.type} onValueChange={(v) => setTrade((t) => ({ ...t, type: v as TradeType }))}>
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {(Object.keys(TRADE_TYPE_LABELS) as TradeType[]).map((t) => (
                        <SelectItem key={t} value={t}>
                          {TRADE_TYPE_LABELS[t]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </Field>
                <Field>
                  <FieldLabel htmlFor="trade-date">Fecha</FieldLabel>
                  <Input id="trade-date" type="date" max={todayInArgentina()} value={trade.date} onChange={(e) => setTrade((t) => ({ ...t, date: e.target.value }))} />
                </Field>
                <Field>
                  <FieldLabel htmlFor="trade-qty">Cantidad</FieldLabel>
                  <Input id="trade-qty" type="number" step="any" min="0" placeholder="0" value={trade.quantity} onChange={(e) => setTrade((t) => ({ ...t, quantity: e.target.value }))} />
                </Field>
                <Field>
                  <FieldLabel htmlFor="trade-price">Precio (ARS)</FieldLabel>
                  <Input id="trade-price" type="number" step="0.01" min="0" value={trade.priceArs} onChange={(e) => setTrade((t) => ({ ...t, priceArs: e.target.value }))} />
                </Field>
                <Field>
                  <FieldLabel htmlFor="trade-rate">Dólar de ese día</FieldLabel>
                  <Input id="trade-rate" type="number" step="0.01" min="0.01" value={trade.usdRate} onChange={(e) => setTrade((t) => ({ ...t, usdRate: e.target.value }))} />
                </Field>
                <Field>
                  <FieldLabel htmlFor="trade-fee">Comisión (ARS)</FieldLabel>
                  <Input id="trade-fee" type="number" step="0.01" min="0" placeholder="0.00" value={trade.feeArs} onChange={(e) => setTrade((t) => ({ ...t, feeArs: e.target.value }))} />
                </Field>
                <Button className="col-span-2" onClick={handleAddTrade} disabled={isPending}>
                  {`Registrar ${TRADE_TYPE_LABELS[trade.type].toLowerCase()}`}
                </Button>
              </div>
            ) : (
              <div className="grid grid-cols-2 gap-3">
                <Field>
                  <FieldLabel>Tipo</FieldLabel>
                  <Select value={income.type} onValueChange={(v) => setIncome((i) => ({ ...i, type: v as IncomeType }))}>
                    <SelectTrigger className="w-full">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {(Object.keys(INCOME_TYPE_LABELS) as IncomeType[]).map((t) => (
                        <SelectItem key={t} value={t}>
                          {INCOME_TYPE_LABELS[t]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </Field>
                <Field>
                  <FieldLabel htmlFor="income-date">Fecha</FieldLabel>
                  <Input id="income-date" type="date" max={todayInArgentina()} value={income.date} onChange={(e) => setIncome((i) => ({ ...i, date: e.target.value }))} />
                </Field>
                <Field>
                  <FieldLabel htmlFor="income-amount">Monto</FieldLabel>
                  <div className="flex gap-2">
                    <Input id="income-amount" type="number" step="0.01" min="0" placeholder="0.00" value={income.amount} onChange={(e) => setIncome((i) => ({ ...i, amount: e.target.value }))} />
                    <div className="flex shrink-0 rounded-md border p-0.5 text-xs" role="group" aria-label="Moneda del cobro">
                      {(["USD", "ARS"] as const).map((c) => (
                        <button
                          key={c}
                          type="button"
                          aria-pressed={income.currency === c}
                          onClick={() => setIncome((i) => ({ ...i, currency: c }))}
                          className={`rounded px-2 ${income.currency === c ? "bg-primary text-primary-foreground" : "text-muted-foreground"}`}
                        >
                          {c}
                        </button>
                      ))}
                    </div>
                  </div>
                </Field>
                {income.currency === "ARS" && (
                  <Field>
                    <FieldLabel htmlFor="income-rate">Dólar de ese día</FieldLabel>
                    <Input id="income-rate" type="number" step="0.01" min="0.01" value={income.usdRate} onChange={(e) => setIncome((i) => ({ ...i, usdRate: e.target.value }))} />
                  </Field>
                )}
                <Button className="col-span-2" onClick={handleAddIncome} disabled={isPending}>
                  Registrar cobro
                </Button>
              </div>
            )}
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>

          <section className="flex flex-col gap-2">
            <h3 className="text-sm font-semibold text-muted-foreground">Historial</h3>
            {!activity ? (
              <p className="text-sm text-muted-foreground">{isPending ? "Cargando…" : "—"}</p>
            ) : activity.trades.length + activity.incomes.length === 0 ? (
              <p className="text-sm text-muted-foreground">Todavía no hay operaciones ni cobros.</p>
            ) : (
              <div className="overflow-x-auto rounded-lg border">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50 text-left text-muted-foreground">
                    <tr>
                      <th className="px-3 py-2 font-medium">Movimiento</th>
                      <th className="px-3 py-2 text-right font-medium">Detalle</th>
                      <th className="px-3 py-2 text-right font-medium">USD</th>
                      <th className="w-10" />
                    </tr>
                  </thead>
                  <tbody>
                    {[
                      ...activity.trades.map((t) => ({ kind: "trade" as const, date: t.date, item: t })),
                      ...activity.incomes.map((i) => ({ kind: "income" as const, date: i.date, item: i })),
                    ]
                      .sort((a, b) => b.date.localeCompare(a.date))
                      .map((row) =>
                        row.kind === "trade" ? (
                          <tr key={row.item.id} className="border-t">
                            <td className="px-3 py-2">
                              <div className="flex flex-col">
                                <span>{row.item.isInitial ? "Posición inicial" : TRADE_TYPE_LABELS[row.item.type]}</span>
                                <span className="text-xs tabular-nums text-muted-foreground">{formatDate(row.item.date)}</span>
                              </div>
                            </td>
                            <td className="px-3 py-2 text-right tabular-nums">
                              <div className="flex flex-col items-end">
                                <span className="whitespace-nowrap">{`${formatQuantity(row.item.quantity)} × ${row.item.priceArs != null ? formatMoney(row.item.priceArs, "ARS") : formatMoney(row.item.priceUsd, "USD")}`}</span>
                                {row.item.usdRate != null && (
                                  <span className="text-xs text-muted-foreground">
                                    {`dólar ${formatMoney(row.item.usdRate, "ARS")}${row.item.feeArs > 0 ? ` · comisión ${formatMoney(row.item.feeArs, "ARS")}` : ""}`}
                                  </span>
                                )}
                              </div>
                            </td>
                            <td className={`whitespace-nowrap px-3 py-2 text-right tabular-nums ${row.item.type === "Sell" ? "text-green-600 dark:text-green-500" : ""}`}>
                              {formatMoney(
                                row.item.type === "Buy"
                                  ? -(row.item.quantity * row.item.priceUsd + row.item.feeUsd)
                                  : row.item.quantity * row.item.priceUsd - row.item.feeUsd,
                                "USD",
                              )}
                            </td>
                            <td className="px-1 py-2">
                              <Button variant="ghost" size="icon-sm" aria-label="Borrar operación" disabled={isPending} onClick={() => confirm("¿Borrar esta operación? Se recalculan la cantidad y el PPC.") && run(() => deleteTradeAction(row.item.id))}>
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </td>
                          </tr>
                        ) : (
                          <tr key={row.item.id} className="border-t">
                            <td className="px-3 py-2">
                              <div className="flex flex-col">
                                <span>{INCOME_TYPE_LABELS[row.item.type]}</span>
                                <span className="text-xs tabular-nums text-muted-foreground">{formatDate(row.item.date)}</span>
                              </div>
                            </td>
                            <td className="px-3 py-2 text-right tabular-nums text-muted-foreground">
                              {row.item.amountArs != null ? (
                                <div className="flex flex-col items-end">
                                  <span>{formatMoney(row.item.amountArs, "ARS")}</span>
                                  <span className="text-xs">{`dólar ${formatMoney(row.item.usdRate ?? 0, "ARS")}`}</span>
                                </div>
                              ) : (
                                "En dólares"
                              )}
                            </td>
                            <td className="whitespace-nowrap px-3 py-2 text-right tabular-nums text-green-600 dark:text-green-500">
                              {formatMoney(row.item.amountUsd, "USD")}
                            </td>
                            <td className="px-1 py-2">
                              <Button variant="ghost" size="icon-sm" aria-label="Borrar cobro" disabled={isPending} onClick={() => confirm("¿Borrar este cobro?") && run(() => deleteIncomeAction(row.item.id))}>
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </td>
                          </tr>
                        ),
                      )}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </div>
      </SheetContent>
    </Sheet>
  );
}

function Stat({ label, value, valueClass = "" }: { label: string; value: string; valueClass?: string }) {
  return (
    <div className="flex flex-col gap-0.5 rounded-md bg-muted/50 px-3 py-2">
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className={`font-semibold tabular-nums ${valueClass}`}>{value}</dd>
    </div>
  );
}
