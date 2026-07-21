import { ReportLine } from "../types/report-line";
import { formatCurrency } from "@/src/features/accounts/utils/format";

interface ReportLineListProps {
  title: string;
  lines: ReportLine[];
  emptyLabel?: string;
}

export default function ReportLineList({
  title,
  lines,
  emptyLabel = "Sin movimientos",
}: ReportLineListProps) {
  return (
    <div className="flex flex-col gap-2">
      <h3 className="text-sm font-semibold text-muted-foreground">{title}</h3>
      {lines.length === 0 ? (
        <p className="text-sm text-muted-foreground py-4">{emptyLabel}</p>
      ) : (
        lines.map((line) => (
          <div
            key={line.accountId}
            className="flex items-center justify-between rounded-lg border bg-card px-4 py-3"
          >
            <span className="font-medium">{line.accountName}</span>
            <span className="text-sm font-semibold tabular-nums">{formatCurrency(line.amount)}</span>
          </div>
        ))
      )}
    </div>
  );
}
