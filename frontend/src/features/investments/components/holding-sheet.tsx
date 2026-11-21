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
import { formatMoney } from "../utils/format";
import { saveHoldingAction } from "../actions/investment-actions";

type CostMode = "ars" | "usd";

interface HoldingSheetProps {
  groups: InvestmentGroup[];
  usdRate: number;
  holding?: InvestmentHolding;
  defaultGroupId?: string;
}

interface Draft {
  groupId: string;
  ticker: string;
  targetPercent: string;
  quantity: string;
  costMode: CostMode;
  costArs: string;
  costRate: string;
  costUsd: string;
  priceArs: string;
}

function draftFrom(
  groups: InvestmentGroup[],
  usdRate: number,
  holding?: InvestmentHolding,
  defaultGroupId?: string,
): Draft {
  return {
    groupId: holding?.groupId ?? defaultGroupId ?? groups[0]?.id ?? "",
    ticker: holding?.ticker ?? "",
    targetPercent: holding ? String(holding.targetPercent) : "",
    quantity: holding ? String(holding.quantity) : "",
    // An existing PPC is already in USD; a new one is usually entered in ARS.
    costMode: holding ? "usd" : "ars",
    costArs: "",
    costRate: usdRate ? String(usdRate) : "",
    costUsd: holding ? String(holding.averageCostUsd) : "",
    priceArs: holding ? String(holding.priceArs) : "",
  };
}

function costUsdOf(draft: Draft): number {
  if (draft.costMode === "usd") return Number(draft.costUsd);
  const rate = Number(draft.costRate);
  return rate > 0 ? Number(draft.costArs) / rate : NaN;
}

export default function HoldingSheet({ groups, usdRate, holding, defaultGroupId }: HoldingSheetProps) {
  const isEdit = !!holding;
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<Draft>(() => draftFrom(groups, usdRate, holding, defaultGroupId));
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleOpenChange(next: boolean) {
    if (next) {
      setDraft(draftFrom(groups, usdRate, holding, defaultGroupId));
      setError(null);
    }
    setOpen(next);
  }

  function update<K extends keyof Draft>(key: K, value: Draft[K]) {
    setDraft((d) => ({ ...d, [key]: value }));
  }

  const costUsd = costUsdOf(draft);

  function handleSave() {
    const targetPercent = Number(draft.targetPercent || 0);
    const quantity = Number(draft.quantity || 0);
    const priceArs = Number(draft.priceArs || 0);
    const averageCostUsd = draft.costMode === "ars" && !draft.costArs ? 0 : costUsd;

    if (!draft.groupId) return setError("Elegí un grupo.");
    if (!draft.ticker.trim()) return setError("Ingresá el ticker.");
    if (!(targetPercent >= 0 && targetPercent <= 100)) return setError("El % objetivo debe estar entre 0 y 100.");
    if (!(quantity >= 0)) return setError("La cantidad no puede ser negativa.");
    if (!(averageCostUsd >= 0)) return setError("Revisá el precio de compra y el dólar de compra.");
    if (!(priceArs >= 0)) return setError("El precio no puede ser negativo.");

    setError(null);
    startTransition(async () => {
      const result = await saveHoldingAction(
        {
          groupId: draft.groupId,
          ticker: draft.ticker.trim().toUpperCase(),
          targetPercent,
          quantity,
          averageCostUsd,
          priceArs,
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
            Los precios se cargan en pesos; el PPC se guarda en dólares al valor del día de compra.
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

          <Field>
            <FieldLabel htmlFor="holding-quantity">Cantidad</FieldLabel>
            <Input
              id="holding-quantity"
              type="number"
              step="any"
              min="0"
              placeholder="0"
              value={draft.quantity}
              onChange={(e) => update("quantity", e.target.value)}
            />
          </Field>

          <div className="flex flex-col gap-3 rounded-lg border p-3">
            <div className="flex items-center justify-between gap-2">
              <span className="text-sm font-medium">Precio promedio de compra (PPC)</span>
              <div className="flex rounded-md border p-0.5 text-xs">
                {(["ars", "usd"] as CostMode[]).map((m) => (
                  <button
                    key={m}
                    type="button"
                    onClick={() => update("costMode", m)}
                    className={`rounded px-2 py-0.5 ${
                      draft.costMode === m ? "bg-primary text-primary-foreground" : "text-muted-foreground"
                    }`}
                  >
                    {m.toUpperCase()}
                  </button>
                ))}
              </div>
            </div>
            {draft.costMode === "ars" ? (
              <>
                <div className="grid grid-cols-2 gap-3">
                  <Field>
                    <FieldLabel htmlFor="holding-cost-ars">PPC (ARS)</FieldLabel>
                    <Input
                      id="holding-cost-ars"
                      type="number"
                      step="0.01"
                      min="0"
                      placeholder="0.00"
                      value={draft.costArs}
                      onChange={(e) => update("costArs", e.target.value)}
                    />
                  </Field>
                  <Field>
                    <FieldLabel htmlFor="holding-cost-rate">Dólar de compra</FieldLabel>
                    <Input
                      id="holding-cost-rate"
                      type="number"
                      step="0.01"
                      min="0.01"
                      placeholder="0.00"
                      value={draft.costRate}
                      onChange={(e) => update("costRate", e.target.value)}
                    />
                  </Field>
                </div>
                <FieldDescription>
                  {Number.isFinite(costUsd) && draft.costArs
                    ? `Se guarda como ${formatMoney(costUsd, "USD")}`
                    : "Si compraste en varias veces, usá el promedio en pesos y el dólar promedio."}
                </FieldDescription>
              </>
            ) : (
              <Field>
                <FieldLabel htmlFor="holding-cost-usd">PPC (USD)</FieldLabel>
                <Input
                  id="holding-cost-usd"
                  type="number"
                  step="0.0001"
                  min="0"
                  placeholder="0.00"
                  value={draft.costUsd}
                  onChange={(e) => update("costUsd", e.target.value)}
                />
              </Field>
            )}
          </div>

          <Field>
            <FieldLabel htmlFor="holding-price">Precio hoy (ARS)</FieldLabel>
            <Input
              id="holding-price"
              type="number"
              step="0.01"
              min="0"
              placeholder="0.00"
              value={draft.priceArs}
              onChange={(e) => update("priceArs", e.target.value)}
            />
            {usdRate > 0 && Number(draft.priceArs) > 0 && (
              <FieldDescription>
                {`≈ ${formatMoney(Number(draft.priceArs) / usdRate, "USD")} al dólar de hoy`}
              </FieldDescription>
            )}
          </Field>

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
