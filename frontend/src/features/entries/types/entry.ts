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

export interface PaginatedEntries {
  entries: Entry[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
