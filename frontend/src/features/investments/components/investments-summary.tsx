import { InvestmentPortfolio } from "../types/investment";
import { DisplayCurrency, formatMoney, formatPercent, gainClass } from "../utils/format";

interface InvestmentsSummaryProps {
  portfolio: InvestmentPortfolio;
  currency: DisplayCurrency;
  convert: (usd: number) => number;
}

export default function InvestmentsSummary({ portfolio, currency, convert }: InvestmentsSummaryProps) {
  const updated = portfolio.updatedAt
    ? new Date(portfolio.updatedAt).toLocaleDateString("es-AR")
    : "—";
  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
      <Tile label="Valor actual" value={formatMoney(convert(portfolio.valueUsd), currency)}>
        {`Objetivo ${formatMoney(convert(portfolio.targetAmountUsd), currency)}`}
      </Tile>
      <Tile label="Costo" value={formatMoney(convert(portfolio.costUsd), currency)}>
        A precio promedio de compra
      </Tile>
      <Tile
        label="Ganado"
        value={formatMoney(convert(portfolio.gainUsd), currency)}
        valueClass={gainClass(portfolio.gainUsd)}
      >
        <span className={gainClass(portfolio.gainUsd)}>{formatPercent(portfolio.gainPercent)}</span>
      </Tile>
      <Tile
        label="Dólar hoy"
        value={portfolio.usdRate ? formatMoney(portfolio.usdRate, "ARS") : "Sin cargar"}
      >
        {`Actualizado ${updated}`}
      </Tile>
    </div>
  );
}

function Tile({
  label,
  value,
  valueClass = "",
  children,
}: {
  label: string;
  value: string;
  valueClass?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1 rounded-lg border p-4">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className={`text-xl font-semibold tabular-nums ${valueClass}`}>{value}</span>
      <span className="text-xs text-muted-foreground">{children}</span>
    </div>
  );
}
