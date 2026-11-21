import { InvestmentGroup, InvestmentPortfolio } from "../types/investment";
import {
  DisplayCurrency,
  formatMoney,
  formatPercent,
  formatQuantity,
  gainClass,
  GROUP_COLORS,
  targetIsOff,
} from "../utils/format";
import ActivitySheet from "./activity-sheet";
import DeleteHoldingButton from "./delete-holding-button";
import GroupSheet from "./group-sheet";
import HoldingSheet from "./holding-sheet";

interface InvestmentsTableProps {
  portfolio: InvestmentPortfolio;
  currency: DisplayCurrency;
  convert: (usd: number) => number;
}

// Positive: bought more than planned; negative: still to invest.
function Deviation({ value, target, currency }: { value: number; target: number; currency: DisplayCurrency }) {
  const diff = value - target;
  if (target === 0 || Math.abs(diff) < 0.005) return null;
  return (
    <span className="text-xs text-muted-foreground">
      {diff > 0 ? "+" : "−"}
      {formatMoney(Math.abs(diff), currency)}
    </span>
  );
}

export default function InvestmentsTable({ portfolio, currency, convert }: InvestmentsTableProps) {
  const money = (usd: number) => formatMoney(convert(usd), currency);
  const th = "px-3 py-2 font-medium whitespace-nowrap";
  const td = "px-3 py-2 tabular-nums whitespace-nowrap";

  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm">
        <thead className="bg-muted/50 text-left text-muted-foreground">
          <tr>
            <th className={th}>Inversión</th>
            <th className={`${th} text-right`}>% objetivo</th>
            <th className={`${th} text-right`}>A invertir</th>
            <th className={`${th} text-right`}>PPC</th>
            <th className={`${th} text-right`}>Precio hoy</th>
            <th className={`${th} text-right`}>Cantidad</th>
            <th className={`${th} text-right`}>Valor</th>
            <th className={`${th} text-right`}>% real</th>
            <th className={`${th} text-right`}>% ganado</th>
            <th className={`${th} text-right`}>Ganado</th>
            <th className={`${th} text-right`} title="Ganancia por precio + ventas + cobros">
              Rend. total
            </th>
            <th className="w-28 px-3 py-2" />
          </tr>
        </thead>
        {portfolio.groups.map((group) => (
          <GroupRows
            key={group.id}
            group={group}
            portfolio={portfolio}
            currency={currency}
            money={money}
            convert={convert}
            td={td}
          />
        ))}
        <tfoot>
          <tr className="border-t-2 bg-muted/50 font-semibold">
            <td className={td}>Total</td>
            <td
              className={`${td} text-right ${
                targetIsOff(portfolio.targetPercent) ? "text-amber-600 dark:text-amber-500" : ""
              }`}
            >
              {formatPercent(portfolio.targetPercent)}
            </td>
            <td className={`${td} text-right`}>
              {money((portfolio.targetAmountUsd * portfolio.targetPercent) / 100)}
            </td>
            <td colSpan={3} />
            <td className={`${td} text-right`}>{money(portfolio.valueUsd)}</td>
            <td className={`${td} text-right`}>{portfolio.valueUsd > 0 ? formatPercent(100) : "—"}</td>
            <td className={`${td} text-right ${gainClass(portfolio.gainPercent)}`}>
              {formatPercent(portfolio.gainPercent)}
            </td>
            <td className={`${td} text-right ${gainClass(portfolio.gainUsd)}`}>{money(portfolio.gainUsd)}</td>
            <td className={`${td} text-right`}>
              <TotalReturn usd={portfolio.totalReturnUsd} percent={portfolio.totalReturnPercent} money={money} />
            </td>
            <td />
          </tr>
        </tfoot>
      </table>
    </div>
  );
}

function GroupRows({
  group,
  portfolio,
  currency,
  money,
  convert,
  td,
}: {
  group: InvestmentGroup;
  portfolio: InvestmentPortfolio;
  currency: DisplayCurrency;
  money: (usd: number) => string;
  convert: (usd: number) => number;
  td: string;
}) {
  const color = GROUP_COLORS[group.color];
  return (
    <tbody className={color.row}>
      <tr className="border-t">
        <td colSpan={12} className="px-3 pt-3 pb-1">
          <div className="flex items-center gap-2">
            <span className={`size-2.5 rounded-full ${color.dot}`} />
            <span className="font-semibold">{group.name}</span>
            <GroupSheet group={group} />
          </div>
        </td>
      </tr>
      {group.holdings.length === 0 && (
        <tr>
          <td colSpan={12} className="px-3 py-2 text-muted-foreground">
            Sin inversiones en este grupo.
          </td>
        </tr>
      )}
      {group.holdings.map((h) => (
        <tr key={h.id} className="border-t border-border/50">
          <td className={`${td} pl-7 font-medium`}>{h.ticker}</td>
          <td className={`${td} text-right`}>{formatPercent(h.targetPercent)}</td>
          <td className={`${td} text-right`}>{money(h.targetAmountUsd)}</td>
          <td className={`${td} text-right`}>{money(h.averageCostUsd)}</td>
          <td className={`${td} text-right`}>
            <div className="flex flex-col items-end">
              <span>{money(h.priceUsd)}</span>
              {currency === "USD" && (
                <span className="text-xs text-muted-foreground">{formatMoney(h.priceArs, "ARS")}</span>
              )}
            </div>
          </td>
          <td className={`${td} text-right`}>{formatQuantity(h.quantity)}</td>
          <td className={`${td} text-right`}>
            <div className="flex flex-col items-end">
              <span>{money(h.valueUsd)}</span>
              <Deviation value={convert(h.valueUsd)} target={convert(h.targetAmountUsd)} currency={currency} />
            </div>
          </td>
          <td className={`${td} text-right`}>{formatPercent(h.actualPercent)}</td>
          <td className={`${td} text-right ${gainClass(h.gainPercent)}`}>{formatPercent(h.gainPercent)}</td>
          <td className={`${td} text-right ${gainClass(h.gainUsd)}`}>{money(h.gainUsd)}</td>
          <td className={`${td} text-right`}>
            <TotalReturn usd={h.totalReturnUsd} percent={h.totalReturnPercent} money={money} />
          </td>
          <td className="px-3 py-2">
            <div className="flex justify-end gap-1">
              <ActivitySheet holding={h} usdRate={portfolio.usdRate} />
              <HoldingSheet groups={portfolio.groups} usdRate={portfolio.usdRate} holding={h} />
              <DeleteHoldingButton holding={h} />
            </div>
          </td>
        </tr>
      ))}
      <tr className="border-t border-border/50 font-semibold">
        <td className={`${td} pl-7 text-muted-foreground`}>Subtotal</td>
        <td className={`${td} text-right`}>{formatPercent(group.targetPercent)}</td>
        <td className={`${td} text-right`}>{money(group.targetAmountUsd)}</td>
        <td colSpan={3} />
        <td className={`${td} text-right`}>
          <div className="flex flex-col items-end">
            <span>{money(group.valueUsd)}</span>
            <Deviation value={convert(group.valueUsd)} target={convert(group.targetAmountUsd)} currency={currency} />
          </div>
        </td>
        <td className={`${td} text-right`}>{formatPercent(group.actualPercent)}</td>
        <td className={`${td} text-right ${gainClass(group.gainPercent)}`}>{formatPercent(group.gainPercent)}</td>
        <td className={`${td} text-right ${gainClass(group.gainUsd)}`}>{money(group.gainUsd)}</td>
        <td className={`${td} text-right`}>
          <TotalReturn usd={group.totalReturnUsd} percent={group.totalReturnPercent} money={money} />
        </td>
        <td />
      </tr>
    </tbody>
  );
}

function TotalReturn({ usd, percent, money }: { usd: number; percent: number; money: (usd: number) => string }) {
  return (
    <div className={`flex flex-col items-end ${gainClass(usd)}`}>
      <span>{money(usd)}</span>
      <span className="text-xs">{formatPercent(percent)}</span>
    </div>
  );
}
