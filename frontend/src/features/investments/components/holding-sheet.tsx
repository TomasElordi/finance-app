"use client";

import { useState, useTransition } from "react";
import { Pencil, Plus } from "lucide-react";
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
import { Field, FieldDescription, FieldLabel } from "@/src/shared/components/ui/field";
import { Separator } from "@/src/shared/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import { InvestmentGroup, InvestmentHolding } from "../types/investment";
import { formatMoney, todayInArgentina } from "../utils/format";
import { saveHoldingAction } from "../actions/investment-actions";

interface HoldingSheetProps {
  groups: InvestmentGroup[];
  usdRate: number;
  holding?: InvestmentHolding;
}

interface Draft {
  groupId: string;
  ticker: string;
  marketSymbol: string;
  autoPrice: boolean;
  targetPercent: string;
  priceArs: string;
  // First buy, only when creating.
  buyQuantity: string;
  buyPriceArs: string;
  buyUsdRate: string;
  buyFeeArs: string;
  buyDate: string;
}

function draftFrom(groups: InvestmentGroup[], usdRate: number, holding?: InvestmentHolding): Draft {
  return {
    groupId: holding?.groupId ?? groups[0]?.id ?? "",
    ticker: holding?.ticker ?? "",
    marketSymbol: holding?.marketSymbol ?? "",
    autoPrice: holding?.autoPrice ?? true,
    targetPercent: holding ? String(holding.targetPercent) : "",
    priceArs: holding ? String(holding.priceArs) : "",
    buyQuantity: "",
    buyPriceArs: "",
    buyUsdRate: usdRate ? String(usdRate) : "",
    buyFeeArs: "",
    buyDate: todayInArgentina(),
  };
}

export default function HoldingSheet({ groups, usdRate, holding }: HoldingSheetProps) {
  const isEdit = !!holding;
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<Draft>(() => draftFrom(groups, usdRate, holding));
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleOpenChange(next: boolean) {
    if (next) {
      setDraft(draftFrom(groups, usdRate, holding));
      setError(null);
    }
    setOpen(next);
  }

  function update<K extends keyof Draft>(key: K, value: Draft[K]) {
    setDraft((d) => ({ ...d, [key]: value }));
  }

  const buyQuantity = Number(draft.buyQuantity || 0);
  const buyPriceArs = Number(draft.buyPriceArs || 0);
  const buyRate = Number(draft.buyUsdRate || 0);
  const buyFee = Number(draft.buyFeeArs || 0);
  const hasBuy = !isEdit && buyQuantity > 0;
  const buyPpcUsd = hasBuy && buyRate > 0 ? (buyQuantity * buyPriceArs + buyFee) / buyQuantity / buyRate : NaN;

  function handleSave() {
    const targetPercent = Number(draft.targetPercent || 0);
    const priceArs = Number(draft.priceArs || (hasBuy ? draft.buyPriceArs : 0) || 0);

    if (!draft.groupId) return setError("Elegí un grupo.");
    if (!draft.ticker.trim()) return setError("Ingresá el ticker.");
    if (!(targetPercent >= 0 && targetPercent <= 100)) return setError("El % objetivo debe estar entre 0 y 100.");
    if (!(priceArs >= 0)) return setError("El precio no puede ser negativo.");
    if (hasBuy) {
      if (!(buyPriceArs >= 0)) return setError("Revisá el precio de compra.");
      if (!(buyRate > 0)) return setError("Ingresá el dólar del día de compra.");
      if (!(buyFee >= 0)) return setError("La comisión no puede ser negativa.");
    }

    setError(null);
    startTransition(async () => {
      const result = await saveHoldingAction(
        {
          groupId: draft.groupId,
          ticker: draft.ticker.trim().toUpperCase(),
          marketSymbol: draft.marketSymbol.trim().toUpperCase() || null,
          autoPrice: draft.autoPrice,
          targetPercent,
          priceArs,
          initialTrade: hasBuy
            ? {
                date: draft.buyDate,
                type: "Buy",
                quantity: buyQuantity,
                priceArs: buyPriceArs,
                usdRate: buyRate,
                feeArs: buyFee,
              }
            : undefined,
        },
        holding?.id,
      );
      if (result.status === "error") return setError(result.message);
      setOpen(false);
    });
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        {isEdit ? (
          <Button variant="ghost" size="icon-sm" aria-label={`Editar ${holding.ticker}`}>
            <Pencil className="h-4 w-4" />
          </Button>
        ) : (
          <Button disabled={groups.length === 0}>
            <Plus className="h-4 w-4" />
            Nueva inversión
          </Button>
        )}
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>{isEdit ? `Editar ${holding.ticker}` : "Nueva inversión"}</SheetTitle>
          <SheetDescription>
            {isEdit
              ? "La cantidad y el PPC salen de las operaciones: registralas desde el historial."
              : "Los precios se cargan en pesos. La cantidad y el PPC salen de las operaciones."}
          </SheetDescription>
        </SheetHeader>

        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel>Grupo</FieldLabel>
            <Select value={draft.groupId} onValueChange={(v) => update("groupId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue placeholder="Elegí un grupo" />
              </SelectTrigger>
              <SelectContent>
                {groups.map((g) => (
                  <SelectItem key={g.id} value={g.id}>
                    {g.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <div className="grid grid-cols-2 gap-3">
            <Field>
              <FieldLabel htmlFor="holding-ticker">Ticker</FieldLabel>
              <Input
                id="holding-ticker"
                value={draft.ticker}
                maxLength={20}
                placeholder="Ej: GLD"
                onChange={(e) => update("ticker", e.target.value.toUpperCase())}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="holding-target">% objetivo</FieldLabel>
              <Input
                id="holding-target"
                type="number"
                step="0.01"
                min="0"
                max="100"
                placeholder="0"
                value={draft.targetPercent}
                onChange={(e) => update("targetPercent", e.target.value)}
              />
            </Field>
          </div>

          <div className="flex flex-col gap-3 rounded-lg border p-3">
            <span className="text-sm font-medium">Cotización</span>
            <div className="grid grid-cols-2 gap-3">
              <Field>
                <FieldLabel htmlFor="holding-price">Precio hoy (ARS)</FieldLabel>
                <Input
                  id="holding-price"
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder={hasBuy ? "El de compra" : "0.00"}
                  value={draft.priceArs}
                  onChange={(e) => update("priceArs", e.target.value)}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="holding-symbol">Símbolo en IOL</FieldLabel>
                <Input
                  id="holding-symbol"
                  value={draft.marketSymbol}
                  maxLength={20}
                  placeholder={draft.ticker || "Igual al ticker"}
                  onChange={(e) => update("marketSymbol", e.target.value.toUpperCase())}
                />
              </Field>
            </div>
            <FieldDescription>
              Completá el símbolo solo si en el mercado se llama distinto (ej: SP500 → SPY, YPF → YPFD).
            </FieldDescription>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                className="size-4 accent-primary"
                checked={draft.autoPrice}
                onChange={(e) => update("autoPrice", e.target.checked)}
              />
              Actualizar con &quot;Traer cotizaciones&quot;
            </label>
          </div>

          {!isEdit && (
            <div className="flex flex-col gap-3 rounded-lg border p-3">
              <span className="text-sm font-medium">Compra inicial (opcional)</span>
              <div className="grid grid-cols-2 gap-3">
                <Field>
                  <FieldLabel htmlFor="buy-quantity">Cantidad</FieldLabel>
                  <Input
                    id="buy-quantity"
                    type="number"
                    step="any"
                    min="0"
                    placeholder="0"
                    value={draft.buyQuantity}
                    onChange={(e) => update("buyQuantity", e.target.value)}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="buy-date">Fecha</FieldLabel>
                  <Input
                    id="buy-date"
                    type="date"
                    max={todayInArgentina()}
                    value={draft.buyDate}
                    onChange={(e) => update("buyDate", e.target.value)}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="buy-price">Precio (ARS)</FieldLabel>
                  <Input
                    id="buy-price"
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    value={draft.buyPriceArs}
                    onChange={(e) => update("buyPriceArs", e.target.value)}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="buy-rate">Dólar de ese día</FieldLabel>
                  <Input
                    id="buy-rate"
                    type="number"
                    step="0.01"
                    min="0.01"
                    placeholder="0.00"
                    value={draft.buyUsdRate}
                    onChange={(e) => update("buyUsdRate", e.target.value)}
                  />
                </Field>
                <Field>
                  <FieldLabel htmlFor="buy-fee">Comisión (ARS)</FieldLabel>
                  <Input
                    id="buy-fee"
                    type="number"
                    step="0.01"
                    min="0"
                    placeholder="0.00"
                    value={draft.buyFeeArs}
                    onChange={(e) => update("buyFeeArs", e.target.value)}
                  />
                </Field>
              </div>
              {Number.isFinite(buyPpcUsd) && (
                <FieldDescription>{`PPC resultante: ${formatMoney(buyPpcUsd, "USD")} (comisión incluida)`}</FieldDescription>
              )}
            </div>
          )}

          {error && <p className="text-sm text-destructive">{error}</p>}
          <Separator />
          <Button onClick={handleSave} disabled={isPending}>
            {isPending ? "Guardando..." : isEdit ? "Guardar cambios" : "Agregar"}
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
