"use client";

import { useState } from "react";
import { AlertTriangle, TrendingUp } from "lucide-react";
import { InvestmentPortfolio, InvestmentSnapshot } from "../types/investment";
import { DisplayCurrency, formatPercent, targetIsOff } from "../utils/format";
import ContributionSheet from "./contribution-sheet";
import GroupSheet from "./group-sheet";
import HoldingSheet from "./holding-sheet";
import InvestmentsHistoryChart from "./investments-history-chart";
import InvestmentsSummary from "./investments-summary";
import InvestmentsTable from "./investments-table";
import PortfolioSheet from "./portfolio-sheet";
import PricesSheet from "./prices-sheet";

export default function InvestmentsView({
  portfolio,
  history,
}: {
  portfolio: InvestmentPortfolio;
  history: InvestmentSnapshot[];
}) {
  const [currency, setCurrency] = useState<DisplayCurrency>("USD");
  // Everything comes in USD; ARS is shown at today's rate.
  const convert = (usd: number) => (currency === "ARS" ? usd * portfolio.usdRate : usd);
  const holdingCount = portfolio.groups.reduce((n, g) => n + g.holdings.length, 0);
  const targetOff = holdingCount > 0 && targetIsOff(portfolio.targetPercent);

  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold">Inversiones</h1>
        <div className="flex flex-wrap items-center gap-2">
          <div className="flex rounded-md border p-0.5 text-sm" role="group" aria-label="Moneda">
            {(["USD", "ARS"] as DisplayCurrency[]).map((c) => (
              <button
                key={c}
                type="button"
                aria-pressed={currency === c}
                disabled={c === "ARS" && !portfolio.usdRate}
                onClick={() => setCurrency(c)}
                className={`rounded px-3 py-1 disabled:opacity-50 ${
                  currency === c ? "bg-primary text-primary-foreground" : "text-muted-foreground"
                }`}
              >
                {c}
              </button>
            ))}
          </div>
          <PortfolioSheet portfolio={portfolio} />
          <PricesSheet portfolio={portfolio} />
          <ContributionSheet portfolio={portfolio} />
          <GroupSheet />
          <HoldingSheet groups={portfolio.groups} usdRate={portfolio.usdRate} />
        </div>
      </div>

      {portfolio.groups.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 py-24 text-center">
          <div className="rounded-full bg-muted p-4">
            <TrendingUp className="h-8 w-8 text-muted-foreground" />
          </div>
          <div className="flex flex-col gap-1">
            <p className="font-medium">Todavía no cargaste inversiones</p>
            <p className="text-sm text-muted-foreground">
              Configurá el monto a invertir y el dólar, creá un grupo (ej: Acciones) y agregá tus
              inversiones.
            </p>
          </div>
        </div>
      ) : (
        <>
          <InvestmentsSummary portfolio={portfolio} currency={currency} convert={convert} />
          <InvestmentsHistoryChart history={history} currency={currency} />
          {targetOff && (
            <div className="flex items-start gap-2 rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-900 dark:border-amber-900 dark:bg-amber-950/40 dark:text-amber-200">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <span>
                {`Los % objetivo suman ${formatPercent(portfolio.targetPercent)}, no 100%. `}
                {portfolio.targetPercent < 100
                  ? `Queda ${formatPercent(100 - portfolio.targetPercent)} sin asignar.`
                  : `Sobran ${formatPercent(portfolio.targetPercent - 100)}.`}
              </span>
            </div>
          )}
          <InvestmentsTable portfolio={portfolio} currency={currency} convert={convert} />
        </>
      )}
    </div>
  );
}
