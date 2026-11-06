export const PAGES = {
  HOME: "/home",
  LOGIN: "/login",
  REGISTER: "/register",
  ACCOUNTS: "/accounts",
  ENTRIES: "/entries",
  REPORTS: "/reports",
  BUDGET: "/budget",
  EXPENSES: "/expenses",
  CARD_EXPENSES: "/card-expenses",
  SHARED_EXPENSES: "/shared-expenses",
};

export type Page = (typeof PAGES)[keyof typeof PAGES];

export const PROTECTED_PAGES = new Set<Page>([
  PAGES.HOME,
  PAGES.ACCOUNTS,
  PAGES.ENTRIES,
  PAGES.REPORTS,
  PAGES.BUDGET,
  PAGES.EXPENSES,
  PAGES.CARD_EXPENSES,
  PAGES.SHARED_EXPENSES,
]);

export const AUTH_PAGES = new Set<Page>([PAGES.LOGIN, PAGES.REGISTER]);
