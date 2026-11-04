export type CardItemType = "Expense" | "Tax" | "Credit";

export const CARD_ITEM_TYPE_LABELS: Record<CardItemType, string> = {
  Expense: "Gasto",
  Tax: "Impuesto",
  Credit: "Saldo a favor",
};

export interface CardItem {
  id: string;
  name: string;
  amount: number;
  type: CardItemType;
  accountId: string | null;
  accountName: string | null;
  personId: string | null;
  personName: string;
  installmentNumber: number;
  installments: number;
}

export interface CardPersonTotal {
  personId: string | null;
  personName: string;
  expenses: number;
  taxes: number;
  credits: number;
  total: number;
}

export interface CardStatement {
  year: number;
  month: number;
  items: CardItem[];
  people: CardPersonTotal[];
  total: number;
}

export interface CardUpcoming {
  total: number;
  people: { personId: string | null; personName: string; total: number }[];
  months: { year: number; month: number; amount: number }[];
}

export interface Person {
  id: string;
  name: string;
}

export interface CardItemInput {
  name: string;
  amount: number;
  type: CardItemType;
  accountId: string | null;
  personId: string | null;
  year: number;
  month: number;
  currentInstallment: number;
  installments: number;
}
