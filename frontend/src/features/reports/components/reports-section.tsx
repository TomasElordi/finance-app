import { getIncomeStatement } from "../data/get-income-statement";
import { getBalanceSheet } from "../data/get-balance-sheet";
import ReportsView from "./reports-view";

interface ReportsSectionProps {
  tab: "income" | "balance";
  year: number;
  month: number;
  bsYear: number;
}

export default async function ReportsSection({ tab, year, month, bsYear }: ReportsSectionProps) {
  const [incomeStatement, balanceSheet] = await Promise.all([
    tab === "income"
      ? getIncomeStatement(year, month)
      : { year, month, income: [], expenses: [], totalIncome: 0, totalExpenses: 0, netResult: 0 },
    tab === "balance"
      ? getBalanceSheet(bsYear)
      : { year: bsYear, assets: [], liabilities: [], equity: [], totalAssets: 0, totalLiabilities: 0, totalEquity: 0 },
  ]);

  return (
    <ReportsView
      tab={tab}
      year={year}
      month={month}
      bsYear={bsYear}
      incomeStatement={incomeStatement}
      balanceSheet={balanceSheet}
    />
  );
}
