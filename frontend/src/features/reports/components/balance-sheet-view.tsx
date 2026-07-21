import { BalanceSheet } from "../types/balance-sheet";
import { Card, CardContent, CardHeader, CardTitle } from "@/src/shared/components/ui/card";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { cn } from "@/src/shared/lib/utils";
import { Landmark, Scale, Wallet } from "lucide-react";
import ReportLineList from "./report-line-list";

export default function BalanceSheetView({
  assets,
  liabilities,
  equity,
  totalAssets,
  totalLiabilities,
  totalEquity,
}: BalanceSheet) {
  const cards = [
    { title: "Total Activo", value: totalAssets, icon: Wallet, colorClass: "text-blue-500" },
    { title: "Total Pasivo", value: totalLiabilities, icon: Landmark, colorClass: "text-red-500" },
    { title: "Total Patrimonio", value: totalEquity, icon: Scale, colorClass: "text-emerald-500" },
  ];

  return (
    <div className="flex flex-col gap-6">
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
        {cards.map((card) => (
          <Card key={card.title}>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">{card.title}</CardTitle>
              <card.icon className={cn("size-4", card.colorClass)} />
            </CardHeader>
            <CardContent>
              <span className="text-xl font-bold">{formatCurrency(card.value)}</span>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <ReportLineList title="Activo" lines={assets} />
        <ReportLineList title="Pasivo" lines={liabilities} />
        <ReportLineList title="Patrimonio" lines={equity} />
      </div>
    </div>
  );
}
