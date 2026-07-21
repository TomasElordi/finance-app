import { IncomeStatement } from "../types/income-statement";
import { BalanceSheet } from "../types/balance-sheet";
import PeriodSelector from "@/src/features/budget/components/period-selector";
import YearSelector from "./year-selector";
import ReportsTabs from "./reports-tabs";
import IncomeStatementView from "./income-statement-view";
import BalanceSheetView from "./balance-sheet-view";

interface ReportsViewProps {
  tab: "income" | "balance";
  year: number;
  month: number;
  bsYear: number;
  incomeStatement: IncomeStatement;
  balanceSheet: BalanceSheet;
}

export default function ReportsView({
  tab,
  year,
  month,
  bsYear,
  incomeStatement,
  balanceSheet,
}: ReportsViewProps) {
  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Reportes</h1>
        {tab === "income" ? <PeriodSelector year={year} month={month} /> : <YearSelector year={bsYear} />}
      </div>

      <ReportsTabs tab={tab} year={year} month={month} bsYear={bsYear} />

      {tab === "income" ? (
        <IncomeStatementView {...incomeStatement} />
      ) : (
        <BalanceSheetView {...balanceSheet} />
      )}
    </div>
  );
}
