export interface MonthlyAmount {
  year: number;
  month: number;
  amount: number;
}

export interface AccountMovement {
  entryId: string;
  entryTitle: string;
  date: string;
  type: "Credit" | "Debit";
  amount: number;
}
