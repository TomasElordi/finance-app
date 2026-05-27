export enum NatureType {
  Asset = 0,
  Liability = 1,
  Equity = 2,
  Income = 3,
  Expense = 4,
}

export interface Account {
  id: string;
  userId: string;
  name: string;
  nature: string;
}
