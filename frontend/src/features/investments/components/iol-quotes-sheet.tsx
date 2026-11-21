"use client";

import { useState, useTransition } from "react";
import { CloudDownload, ShieldCheck } from "lucide-react";
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
import { InvestmentPortfolio, QuotesResult } from "../types/investment";
import { formatMoney } from "../utils/format";
import { fetchIolQuotesAction } from "../actions/investment-actions";

// Asks for the IOL credentials every time: they're sent once to fetch prices and kept
// nowhere on our side (not in the database, and dropped from this component once used).
// The fields keep their values until the request finishes and the form is replaced by the
// result, which is what browsers look for to offer saving the login for autofill.
export default function IolQuotesSheet({ portfolio }: { portfolio: InvestmentPortfolio }) {
  const autoCount = portfolio.groups.reduce((n, g) => n + g.holdings.filter((h) => h.autoPrice).length, 0);
  const [open, setOpen] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [result, setResult] = useState<QuotesResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleOpenChange(next: boolean) {
    setUsername("");
    setPassword("");
    setResult(null);
    setError(null);
    setOpen(next);
  }

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!username.trim() || !password) return setError("Ingresá tu usuario y contraseña de IOL.");
    setError(null);
    startTransition(async () => {
      const response = await fetchIolQuotesAction(username.trim(), password);
      if (response.status === "error") {
        setPassword("");
        setError(response.message);
        return;
      }
      setResult(response.result);
      // The form is gone by now; drop the credentials from memory too.
      setTimeout(() => {
        setUsername("");
        setPassword("");
      }, 0);
    });
  }

  return (
    // Non-modal: a modal sheet blocks pointer events outside it, and password-manager
    // extensions (iCloud Passwords, 1Password...) draw their autofill menu outside it.
    // Clicks outside don't close it either, so picking a login from that menu works;
    // it closes with the X or Esc.
    <Sheet open={open} onOpenChange={handleOpenChange} modal={false}>
      <SheetTrigger asChild>
        <Button variant="outline" disabled={autoCount === 0}>
          <CloudDownload className="h-4 w-4" />
          Traer cotizaciones
        </Button>
      </SheetTrigger>
      <SheetContent className="overflow-y-auto shadow-2xl sm:max-w-md" onInteractOutside={(e) => e.preventDefault()}>
        <SheetHeader>
          <SheetTitle>Traer cotizaciones de IOL</SheetTitle>
          <SheetDescription>
            {`Actualiza el precio de ${autoCount} inversiones y el dólar MEP.`}
          </SheetDescription>
        </SheetHeader>
        <div className="flex flex-col gap-4 p-4">
          {!result ? (
            <form className="flex flex-col gap-4" method="post" onSubmit={handleSubmit}>
              <div className="flex gap-2 rounded-lg border p-3 text-xs text-muted-foreground">
                <ShieldCheck className="size-4 shrink-0" />
                <span>
                  Tus credenciales se usan solo para esta consulta y no se guardan en ningún lado. Vas a
                  tener que ingresarlas cada vez.
                </span>
              </div>
              <Field>
                <FieldLabel htmlFor="iol-user">Usuario de IOL</FieldLabel>
                <Input
                  id="iol-user"
                  name="username"
                  autoComplete="username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="iol-pass">Contraseña</FieldLabel>
                <Input
                  id="iol-pass"
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </Field>
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Button type="submit" disabled={isPending}>
                {isPending ? "Consultando IOL..." : "Traer cotizaciones"}
              </Button>
            </form>
          ) : (
            <div className="flex flex-col gap-4 text-sm">
              <p>
                {`Se actualizaron ${result.updated.length} precios. `}
                {result.usdRate
                  ? `Dólar MEP: ${formatMoney(result.usdRate, "ARS")}.`
                  : "No se pudo traer el dólar; quedó el anterior."}
              </p>
              {result.updated.length > 0 && (
                <ul className="flex flex-col divide-y rounded-lg border">
                  {result.updated.map((q) => (
                    <li key={q.ticker} className="flex justify-between px-3 py-2">
                      <span className="font-medium">
                        {q.ticker}
                        {q.symbol !== q.ticker && <span className="ml-1 text-xs text-muted-foreground">{q.symbol}</span>}
                      </span>
                      <span className="tabular-nums">{formatMoney(q.priceArs, "ARS")}</span>
                    </li>
                  ))}
                </ul>
              )}
              {result.notFound.length > 0 && (
                <div className="flex flex-col gap-2">
                  <p className="font-medium text-amber-700 dark:text-amber-400">No encontrados (quedó el precio anterior)</p>
                  <ul className="flex flex-col divide-y rounded-lg border">
                    {result.notFound.map((q) => (
                      <li key={q.ticker} className="flex justify-between gap-3 px-3 py-2">
                        <span className="font-medium">{q.symbol}</span>
                        <span className="text-right text-muted-foreground">{q.reason}</span>
                      </li>
                    ))}
                  </ul>
                  <p className="text-xs text-muted-foreground">
                    Si en IOL se llama distinto, editá la inversión y completá el &quot;Símbolo en IOL&quot;.
                  </p>
                </div>
              )}
              <Button variant="outline" onClick={() => handleOpenChange(false)}>
                Cerrar
              </Button>
            </div>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
