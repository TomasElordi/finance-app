import { Account } from "@/src/features/accounts/types/account";
import { Budget } from "../types/budget";
import { BudgetSummaryItem } from "../types/period-summary";
import PeriodSelector from "./period-selector";
import BudgetForm from "./budget-form";
import BudgetCharts from "./budget-charts";
import { Separator } from "@/src/shared/components/ui/separator";

interface BudgetViewProps {
  accounts: Account[];
  budgets: Budget[];
  summary: BudgetSummaryItem[];
  year: number;
  month: number;
}

export default function BudgetView({ accounts, budgets, summary, year, month }: BudgetViewProps) {
  return (
    <div className="p-6 flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Presupuesto</h1>
        <PeriodSelector year={year} month={month} />
      </div>

      <div className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold">Cargar presupuesto</h2>
        <BudgetForm accounts={accounts} budgets={budgets} year={year} month={month} />
      </div>

      {summary.length > 0 && (
        <>
          <Separator />
          <BudgetCharts items={summary} />
        </>
      )}
    </div>
  );
}
