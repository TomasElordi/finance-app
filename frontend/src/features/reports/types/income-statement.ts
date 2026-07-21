import { ReportLine } from "./report-line";

export interface IncomeStatement {
  year: number;
  month: number;
  income: ReportLine[];
  expenses: ReportLine[];
  totalIncome: number;
  totalExpenses: number;
  netResult: number;
}
