import { Account } from "@/src/features/accounts/types/account";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { TrendingDown, TrendingUp, Landmark, Scale } from "lucide-react";
import { cn } from "@/src/shared/lib/utils";

interface SummaryCardsProps {
  accounts: Account[];
}

export default function SummaryCards({ accounts }: SummaryCardsProps) {
  const totalAssets = accounts
    .filter((a) => a.nature === "Asset")
    .reduce((sum, a) => sum + a.balance, 0);

  const totalLiabilities = accounts
    .filter((a) => a.nature === "Liability")
    .reduce((sum, a) => sum + a.balance, 0);

  const netWorth = accounts
    .filter((a) => a.nature === "Equity")
    .reduce((sum, a) => sum + a.balance, 0);

  const totalIncome = accounts
    .filter((a) => a.nature === "Income")
    .reduce((sum, a) => sum + a.balance, 0);

  const totalExpenses = accounts
    .filter((a) => a.nature === "Expense")
    .reduce((sum, a) => sum + a.balance, 0);

  const result = totalIncome - totalExpenses;

  const cards = [
    {
      title: "Activos",
      value: totalAssets,
      icon: Landmark,
      colorClass: "text-blue-500",
    },
    {
      title: "Pasivos",
      value: totalLiabilities,
      icon: Scale,
      colorClass: "text-red-500",
    },
    {
      title: "Patrimonio Neto",
      value: netWorth,
      icon: Landmark,
      colorClass: "text-purple-500",
    },
    {
      title: "Resultado del período",
      value: result,
      icon: result >= 0 ? TrendingUp : TrendingDown,
      colorClass: result >= 0 ? "text-green-500" : "text-red-500",
    },
  ];

  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
      {cards.map((card) => (
        <Card key={card.title}>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {card.title}
            </CardTitle>
            <card.icon className={cn("size-4", card.colorClass)} />
          </CardHeader>
          <CardContent>
            <span
              className={cn(
                "text-xl font-bold",
                card.title === "Resultado del período" && card.colorClass,
              )}
            >
              {formatCurrency(card.value)}
            </span>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
