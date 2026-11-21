"use client";

import { useState, useTransition } from "react";
import { RefreshCw } from "lucide-react";
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
import { Separator } from "@/src/shared/components/ui/separator";
import { InvestmentPortfolio } from "../types/investment";
import { GROUP_COLORS } from "../utils/format";
import { updatePricesAction } from "../actions/investment-actions";

interface PricesSheetProps {
  portfolio: InvestmentPortfolio;
}

// The day-to-day update: today's dollar and every price in ARS, in one go.
export default function PricesSheet({ portfolio }: PricesSheetProps) {
  const holdings = portfolio.groups.flatMap((g) => g.holdings.map((h) => ({ ...h, color: g.color })));
  const [open, setOpen] = useState(false);
  const [rate, setRate] = useState("");
  const [prices, setPrices] = useState<Record<string, string>>({});
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleOpenChange(next: boolean) {
    if (next) {
      setRate(portfolio.usdRate ? String(portfolio.usdRate) : "");
      setPrices(Object.fromEntries(holdings.map((h) => [h.id, String(h.priceArs)])));
      setError(null);
    }
    setOpen(next);
  }

  function handleSave() {
    const usdRate = Number(rate);
    if (!(usdRate > 0)) return setError("El dólar debe ser mayor a cero.");
    const parsed = holdings.map((h) => ({ holdingId: h.id, priceArs: Number(prices[h.id] || 0) }));
    if (parsed.some((p) => !(p.priceArs >= 0))) return setError("Revisá los precios.");
    setError(null);
    startTransition(async () => {
      const result = await updatePricesAction({ usdRate, prices: parsed });
      if (result.status === "error") return setError(result.message);
      setOpen(false);
    });
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        <Button variant="outline" disabled={holdings.length === 0}>
          <RefreshCw className="h-4 w-4" />
          Actualizar precios
        </Button>
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>Actualizar precios</SheetTitle>
          <SheetDescription>Cargá el dólar del día y los precios en pesos.</SheetDescription>
        </SheetHeader>
        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel htmlFor="prices-rate">Dólar hoy (ARS)</FieldLabel>
            <Input
              id="prices-rate"
              type="number"
              step="0.01"
              min="0.01"
              value={rate}
              onChange={(e) => setRate(e.target.value)}
            />
          </Field>
          <Separator />
          <div className="flex flex-col gap-2">
            {holdings.map((h) => (
              <div key={h.id} className="flex items-center gap-3">
                <label htmlFor={`price-${h.id}`} className="flex w-24 shrink-0 items-center gap-2 text-sm font-medium">
                  <span className={`size-2.5 rounded-full ${GROUP_COLORS[h.color].dot}`} />
                  {h.ticker}
                </label>
                <Input
                  id={`price-${h.id}`}
                  type="number"
                  step="0.01"
                  min="0"
                  value={prices[h.id] ?? ""}
                  onChange={(e) => setPrices((p) => ({ ...p, [h.id]: e.target.value }))}
                />
              </div>
            ))}
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <Separator />
          <Button onClick={handleSave} disabled={isPending}>
            {isPending ? "Guardando..." : "Guardar precios"}
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
