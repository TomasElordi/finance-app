export interface SharedGroupSummary {
  id: string;
  name: string;
  memberNames: string[];
  // Positive: the group owes the current user. Negative: the current user owes.
  myBalance: number;
}

export interface SharedMember {
  userId: string;
  name: string;
  isMe: boolean;
  paid: number;
  share: number;
  balance: number;
}

export interface SharedDebt {
  fromUserId: string;
  fromName: string;
  toUserId: string;
  toName: string;
  amount: number;
}

export interface SharedCategory {
  id: string;
  name: string;
}

export interface SharedGroupDetail {
  id: string;
  name: string;
  inviteCode: string;
  members: SharedMember[];
  categories: SharedCategory[];
  debts: SharedDebt[];
}

export interface SharedSplit {
  userId: string;
  name: string;
  amount: number;
}

export interface SharedExpense {
  id: string;
  // ISO date (yyyy-mm-dd)
  date: string;
  description: string;
  total: number;
  categoryId: string | null;
  categoryName: string | null;
  paidByUserId: string;
  paidByName: string;
  splits: SharedSplit[];
}

export interface SharedSettlement {
  id: string;
  date: string;
  fromUserId: string;
  fromName: string;
  toUserId: string;
  toName: string;
  amount: number;
  note: string | null;
}

export interface SharedAmount {
  id: string | null;
  name: string;
  amount: number;
}

export interface SharedPeriod {
  year: number;
  month: number;
  expenses: SharedExpense[];
  settlements: SharedSettlement[];
  total: number;
  shares: SharedAmount[];
  byCategory: SharedAmount[];
}

export interface SharedExpenseInput {
  date: string;
  description: string;
  total: number;
  categoryId: string | null;
  paidByUserId: string;
  splits: { userId: string; amount: number }[];
}

export interface SharedSettlementInput {
  date: string;
  fromUserId: string;
  toUserId: string;
  amount: number;
  note: string | null;
}

export type ActionResult<T = null> =
  | { status: "success"; data: T }
  | { status: "error"; message: string };
