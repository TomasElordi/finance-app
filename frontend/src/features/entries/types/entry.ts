export interface EntryLine {
  id: string;
  accountId: string;
  amount: number;
  type: "Credit" | "Debit";
}

export interface Entry {
  id: string;
  title: string;
  description?: string;
  date: string;
  entryLines: EntryLine[];
}
