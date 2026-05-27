export const PAGES = {
  HOME: "/home",
  LOGIN: "/login",
  REGISTER: "/register",
  ACCOUNTS: "/accounts",
};

export type Page = (typeof PAGES)[keyof typeof PAGES];

export const PROTECTED_PAGES = new Set<Page>([PAGES.HOME, PAGES.ACCOUNTS]);

export const AUTH_PAGES = new Set<Page>([PAGES.LOGIN, PAGES.REGISTER]);
