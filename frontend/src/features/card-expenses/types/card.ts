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
  isRecurring: boolean;
  startYear: number;
  startMonth: number;
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
  isRecurring: boolean;
  // Editing a recurring item: keep earlier months and apply changes from year/month on.
  fromThisMonth: boolean;
}

// A recurring item that was already charged in months before the one being viewed.
export function startedBefore(item: CardItem, year: number, month: number) {
  return item.isRecurring && item.startYear * 12 + item.startMonth < year * 12 + month;
}
