import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import { getBudgets } from "../data/get-budgets";
import { getPeriodSummary } from "../data/get-period-summary";
import { getPeriodOverview } from "../data/get-period-overview";
import BudgetView from "./budget-view";

interface BudgetSectionProps {
  year: number;
  month: number;
}

export default async function BudgetSection({ year, month }: BudgetSectionProps) {
  const previousDate = new Date(year, month - 2, 1);
  const previousYear = previousDate.getFullYear();
  const previousMonth = previousDate.getMonth() + 1;

  const [accounts, budgets, summary, overview, previousBudgets] = await Promise.all([
    getAccounts(),
    getBudgets(year, month),
    getPeriodSummary(year, month),
    getPeriodOverview(year, month),
    getBudgets(previousYear, previousMonth),
  ]);

  return (
    <BudgetView
      accounts={accounts}
      budgets={budgets}
      summary={summary}
      overview={overview}
      year={year}
      month={month}
      hasPreviousBudgets={previousBudgets.length > 0}
    />
  );
}
