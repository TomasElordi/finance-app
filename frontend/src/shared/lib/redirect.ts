import { AUTH_PAGES, Page, PAGES } from "./pages";

export const NEXT_PARAM = "next";

// Where to go after logging in / signing up. Only same-site paths are allowed so the
// `next` param can't be used to send users to another site (open redirect).
export function safeNextPath(value: unknown): string {
  if (typeof value !== "string" || !value.startsWith("/") || value.startsWith("//") || value.includes("\\")) {
    return PAGES.HOME;
  }
  const pathname = value.split(/[?#]/)[0];
  if (AUTH_PAGES.has(pathname as Page)) return PAGES.HOME;
  return value;
}

// Adds ?next=... to an auth page link, unless it would just point home.
export function withNext(page: string, next: string | null | undefined): string {
  const target = safeNextPath(next);
  return target === PAGES.HOME ? page : `${page}?${NEXT_PARAM}=${encodeURIComponent(target)}`;
}
