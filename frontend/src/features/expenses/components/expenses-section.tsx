import { getIncomeStatement } from "@/src/features/reports/data/get-income-statement";
import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import { getExpenseTrend } from "../data/get-expense-trend";
import { getAccountMovements } from "../data/get-account-movements";
import ExpensesView from "./expenses-view";

interface ExpensesSectionProps {
  year: number;
  month: number;
  accountId?: string;
}

export default async function ExpensesSection({ year, month, accountId }: ExpensesSectionProps) {
  const [incomeStatement, accounts, totalTrend, accountTrend, movements] = await Promise.all([
    getIncomeStatement(year, month),
    getAccounts(),
    getExpenseTrend(year, month),
    accountId ? getExpenseTrend(year, month, accountId) : null,
    accountId ? getAccountMovements(accountId, year, month) : null,
  ]);

  const selectedAccount = accountId
    ? accounts.find((a) => a.id === accountId && a.nature === "Expense")
    : undefined;

  return (
    <ExpensesView
      year={year}
      month={month}
      totalExpenses={incomeStatement.totalExpenses}
      byAccount={incomeStatement.expenses}
      totalTrend={totalTrend}
      selectedAccount={
        selectedAccount && accountTrend && movements
          ? { id: selectedAccount.id, name: selectedAccount.name, trend: accountTrend, movements }
          : undefined
      }
    />
  );
}
