import { Account } from "@/src/features/accounts/types/account";
import { Budget } from "../types/budget";
import { BudgetSummaryItem } from "../types/period-summary";
import PeriodSelector from "./period-selector";
import BudgetSheet from "./budget-sheet";
import BudgetCharts from "./budget-charts";
import { Target } from "lucide-react";

interface BudgetViewProps {
  accounts: Account[];
  budgets: Budget[];
  summary: BudgetSummaryItem[];
  year: number;
  month: number;
}

export default function BudgetView({ accounts, budgets, summary, year, month }: BudgetViewProps) {
  const hasExistingBudgets = budgets.length > 0;

  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Presupuesto</h1>
        <div className="flex items-center gap-3">
          <PeriodSelector year={year} month={month} />
          <BudgetSheet
            accounts={accounts}
            budgets={budgets}
            year={year}
            month={month}
            hasExistingBudgets={hasExistingBudgets}
          />
        </div>
      </div>

      {hasExistingBudgets ? (
        <BudgetCharts items={summary} />
      ) : (
        <div className="flex flex-col items-center justify-center gap-4 py-24 text-center">
          <div className="rounded-full bg-muted p-4">
            <Target className="h-8 w-8 text-muted-foreground" />
          </div>
          <div className="flex flex-col gap-1">
            <p className="font-medium">No hay presupuesto para este período</p>
            <p className="text-sm text-muted-foreground">
              Definí cuánto querés gastar en cada categoría y seguí tu progreso.
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
