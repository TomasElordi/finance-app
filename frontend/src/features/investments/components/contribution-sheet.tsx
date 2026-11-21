"use client";

import { useState } from "react";
import { Calculator } from "lucide-react";
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
import { InvestmentPortfolio } from "../types/investment";
import { DisplayCurrency, formatMoney, formatPercent, formatQuantity } from "../utils/format";
import { planContribution } from "../utils/contribution";

export default function ContributionSheet({ portfolio }: { portfolio: InvestmentPortfolio }) {
  const [amount, setAmount] = useState("");
  const [currency, setCurrency] = useState<DisplayCurrency>("USD");
  const rate = portfolio.usdRate;
  const hasHoldings = portfolio.groups.some((g) => g.holdings.length > 0);

  const raw = Number(amount);
  const amountUsd = raw > 0 ? (currency === "ARS" ? (rate > 0 ? raw / rate : 0) : raw) : 0;
  const plan = planContribution(portfolio, amountUsd);
  const both = (usd: number) => (
    <div className="flex flex-col items-end">
      <span>{formatMoney(usd, "USD")}</span>
      {rate > 0 && <span className="text-xs text-muted-foreground">{formatMoney(usd * rate, "ARS")}</span>}
    </div>
  );

  return (
    <Sheet onOpenChange={(open) => open && setAmount("")}>
      <SheetTrigger asChild>
        <Button variant="outline" disabled={!hasHoldings || rate <= 0}>
          <Calculator className="h-4 w-4" />
          Simular aporte
        </Button>
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-xl">
        <SheetHeader>
          <SheetTitle>Simular aporte</SheetTitle>
          <SheetDescription>
            Qué comprar para acercarte a los % objetivo, a los precios de hoy. Solo compras; no se
            guarda nada.
          </SheetDescription>
        </SheetHeader>
        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel htmlFor="contribution-amount">Monto a invertir</FieldLabel>
            <div className="flex gap-2">
              <Input
                id="contribution-amount"
                type="number"
                step="0.01"
                min="0"
                placeholder="0.00"
                autoFocus
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
              />
              <div className="flex shrink-0 rounded-md border p-0.5 text-sm" role="group" aria-label="Moneda del aporte">
                {(["USD", "ARS"] as DisplayCurrency[]).map((c) => (
                  <button
                    key={c}
                    type="button"
                    aria-pressed={currency === c}
                    onClick={() => setCurrency(c)}
                    className={`rounded px-3 ${
                      currency === c ? "bg-primary text-primary-foreground" : "text-muted-foreground"
                    }`}
                  >
                    {c}
                  </button>
                ))}
              </div>
            </div>
          </Field>

          {amountUsd > 0 && plan.lines.length === 0 && (
            <p className="text-sm text-muted-foreground">
              El monto no alcanza para comprar una unidad de lo que está por debajo del objetivo.
            </p>
          )}

          {plan.lines.length > 0 && (
            <>
              <div className="overflow-x-auto rounded-lg border">
                <table className="w-full text-sm">
                  <thead className="bg-muted/50 text-left text-muted-foreground">
                    <tr>
                      <th className="px-3 py-2 font-medium">Comprar</th>
                      <th className="px-3 py-2 text-right font-medium">Cantidad</th>
                      <th className="px-3 py-2 text-right font-medium">Monto</th>
                      <th className="px-3 py-2 text-right font-medium">% después</th>
                    </tr>
                  </thead>
                  <tbody>
                    {plan.lines.map((l) => (
                      <tr key={l.holdingId} className="border-t">
                        <td className="px-3 py-2 font-medium">{l.ticker}</td>
                        <td className="px-3 py-2 text-right tabular-nums">{formatQuantity(l.units)}</td>
                        <td className="px-3 py-2 text-right tabular-nums">{both(l.amountUsd)}</td>
                        <td className="px-3 py-2 text-right tabular-nums">
                          <div className="flex flex-col items-end">
                            <span>{formatPercent(l.percentAfter)}</span>
                            <span className="text-xs text-muted-foreground">
                              {`obj. ${formatPercent(l.targetPercent)}`}
                            </span>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              <dl className="flex flex-col gap-1 rounded-lg bg-muted px-4 py-3 text-sm">
                <div className="flex items-center justify-between gap-2">
                  <dt className="font-medium">Total a invertir</dt>
                  <dd className="tabular-nums font-semibold">{both(plan.investedUsd)}</dd>
                </div>
                {plan.leftoverUsd >= 0.01 && (
                  <div className="flex items-center justify-between gap-2 text-muted-foreground">
                    <dt>Sin asignar (no alcanza para otra unidad)</dt>
                    <dd className="tabular-nums">{both(plan.leftoverUsd)}</dd>
                  </div>
                )}
              </dl>
            </>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
