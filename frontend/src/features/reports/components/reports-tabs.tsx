import Link from "next/link";
import { cn } from "@/src/shared/lib/utils";

interface ReportsTabsProps {
  tab: "income" | "balance";
  year: number;
  month: number;
  bsYear: number;
}

export default function ReportsTabs({ tab, year, month, bsYear }: ReportsTabsProps) {
  const tabs = [
    { key: "income" as const, label: "Estado de Resultados" },
    { key: "balance" as const, label: "Balance General" },
  ];

  return (
    <div className="inline-flex items-center gap-1 rounded-lg border bg-muted/40 p-1 w-fit">
      {tabs.map((t) => (
        <Link
          key={t.key}
          href={`/reports?tab=${t.key}&year=${year}&month=${month}&bsYear=${bsYear}`}
          className={cn(
            "rounded-md px-3 py-1.5 text-sm font-medium transition-colors",
            tab === t.key ? "bg-background shadow-sm" : "text-muted-foreground hover:text-foreground",
          )}
        >
          {t.label}
        </Link>
      ))}
    </div>
  );
}
