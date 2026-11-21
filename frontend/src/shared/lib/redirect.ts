import { AUTH_PAGES, Page, PAGES } from "./pages";

export const NEXT_PARAM = "next";

// Where to go after logging in / signing up. Only same-site paths are allowed so the
// `next` param can't be used to send users to another site (open redirect).
// The value is parsed the way the browser will (which drops tabs and newlines, so a
// string check alone lets "/\t/evil.com" through) and only kept if it stays on our origin.
// The normalized path is returned, never the raw value.
const BASE = "http://local.invalid";

export function safeNextPath(value: unknown): string {
  if (typeof value !== "string" || !value.startsWith("/")) return PAGES.HOME;
  let url: URL;
  try {
    url = new URL(value, BASE);
  } catch {
    return PAGES.HOME;
  }
  // Dot segments can collapse into "//" ("/.//evil.com"), which would be protocol-relative.
  if (url.origin !== BASE || url.pathname.startsWith("//")) return PAGES.HOME;
  if (AUTH_PAGES.has(url.pathname as Page)) return PAGES.HOME;
  return url.pathname + url.search + url.hash;
}

// Adds ?next=... to an auth page link, unless it would just point home.
export function withNext(page: string, next: string | null | undefined): string {
  const target = safeNextPath(next);
  return target === PAGES.HOME ? page : `${page}?${NEXT_PARAM}=${encodeURIComponent(target)}`;
}
