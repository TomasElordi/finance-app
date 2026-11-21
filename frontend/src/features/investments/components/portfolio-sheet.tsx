"use client";

import { useState, useTransition } from "react";
import { Settings2 } from "lucide-react";
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
import { updatePortfolioAction } from "../actions/investment-actions";

interface PortfolioSheetProps {
  portfolio: InvestmentPortfolio;
}

export default function PortfolioSheet({ portfolio }: PortfolioSheetProps) {
  const [open, setOpen] = useState(false);
  const [target, setTarget] = useState("");
  const [rate, setRate] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleOpenChange(next: boolean) {
    if (next) {
      setTarget(portfolio.targetAmountUsd ? String(portfolio.targetAmountUsd) : "");
      setRate(portfolio.usdRate ? String(portfolio.usdRate) : "");
      setError(null);
    }
    setOpen(next);
  }

  function handleSave() {
    const targetAmountUsd = Number(target);
    const usdRate = Number(rate);
    if (!(targetAmountUsd >= 0)) return setError("Ingresá el monto a invertir.");
    if (!(usdRate > 0)) return setError("El dólar debe ser mayor a cero.");
    setError(null);
    startTransition(async () => {
      const result = await updatePortfolioAction({ targetAmountUsd, usdRate });
      if (result.status === "error") return setError(result.message);
      setOpen(false);
    });
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        <Button variant="outline">
          <Settings2 className="h-4 w-4" />
          Configurar
        </Button>
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>Configuración del portfolio</SheetTitle>
          <SheetDescription>
            El monto a invertir se reparte entre las inversiones según su % objetivo.
          </SheetDescription>
        </SheetHeader>
        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel htmlFor="portfolio-target">Monto a invertir (USD)</FieldLabel>
            <Input
              id="portfolio-target"
              type="number"
              step="0.01"
              min="0"
              placeholder="0.00"
              value={target}
              onChange={(e) => setTarget(e.target.value)}
            />
          </Field>
          <Field>
            <FieldLabel htmlFor="portfolio-rate">Dólar hoy (ARS)</FieldLabel>
            <Input
              id="portfolio-rate"
              type="number"
              step="0.01"
              min="0.01"
              placeholder="0.00"
              value={rate}
              onChange={(e) => setRate(e.target.value)}
            />
          </Field>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <Separator />
          <Button onClick={handleSave} disabled={isPending}>
            {isPending ? "Guardando..." : "Guardar"}
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
