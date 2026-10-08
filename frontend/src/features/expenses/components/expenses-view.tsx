import { ReportLine } from "@/src/features/reports/types/report-line";
import PeriodSelector from "@/src/features/budget/components/period-selector";
import { AccountMovement, MonthlyAmount } from "../types/expenses";
import ExpensesTotal from "./expenses-total";
import ExpenseDistribution from "./expense-distribution";
import ExpenseTrendChart from "./expense-trend-chart";
import AccountMovements from "./account-movements";

interface ExpensesViewProps {
  year: number;
  month: number;
  totalExpenses: number;
  byAccount: ReportLine[];
  totalTrend: MonthlyAmount[];
  selectedAccount?: {
    id: string;
    name: string;
    trend: MonthlyAmount[];
    movements: AccountMovement[];
  };
}

export default function ExpensesView({
  year,
  month,
  totalExpenses,
  byAccount,
  totalTrend,
  selectedAccount,
}: ExpensesViewProps) {
  const previous = totalTrend.length >= 2 ? totalTrend[totalTrend.length - 2] : undefined;

  return (
    <div className="p-6 flex flex-col gap-8">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold">Gastos</h1>
        <PeriodSelector year={year} month={month} />
      </div>

      <ExpensesTotal total={totalExpenses} previous={previous} />

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-2">
        <section className="flex min-w-0 flex-col gap-3">
          <h2 className="text-sm font-semibold text-muted-foreground">Por cuenta</h2>
          <ExpenseDistribution
            lines={byAccount}
            total={totalExpenses}
            year={year}
            month={month}
            selectedAccountId={selectedAccount?.id}
          />
        </section>

        <section className="flex min-w-0 flex-col gap-3">
          <h2 className="text-sm font-semibold text-muted-foreground">
            {selectedAccount
              ? `Últimos 12 meses · ${selectedAccount.name}`
              : "Últimos 12 meses · Total"}
          </h2>
          <ExpenseTrendChart
            data={selectedAccount?.trend ?? totalTrend}
            year={year}
            month={month}
            accountId={selectedAccount?.id}
          />
        </section>
      </div>

      {selectedAccount && (
        <section className="flex min-w-0 flex-col gap-3">
          <h2 className="text-sm font-semibold text-muted-foreground">
            Movimientos · {selectedAccount.name}
          </h2>
          <AccountMovements movements={selectedAccount.movements} />
        </section>
      )}
    </div>
  );
}
