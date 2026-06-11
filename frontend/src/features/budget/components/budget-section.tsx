import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import { getBudgets } from "../data/get-budgets";
import { getPeriodSummary } from "../data/get-period-summary";
import BudgetView from "./budget-view";

interface BudgetSectionProps {
  year: number;
  month: number;
}

export default async function BudgetSection({ year, month }: BudgetSectionProps) {
  const [accounts, budgets, summary] = await Promise.all([
    getAccounts(),
    getBudgets(year, month),
    getPeriodSummary(year, month),
  ]);

  return (
    <BudgetView
      accounts={accounts}
      budgets={budgets}
      summary={summary}
      year={year}
      month={month}
    />
  );
}
