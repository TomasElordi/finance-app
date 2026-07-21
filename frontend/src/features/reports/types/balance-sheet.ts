import { ReportLine } from "./report-line";

export interface BalanceSheet {
  year: number;
  assets: ReportLine[];
  liabilities: ReportLine[];
  equity: ReportLine[];
  totalAssets: number;
  totalLiabilities: number;
  totalEquity: number;
}
