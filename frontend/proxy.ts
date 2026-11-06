import { NextRequest, NextResponse } from "next/server";
import {
  AUTH_PAGES,
  Page,
  PAGES,
  PROTECTED_PAGES,
} from "./src/shared/lib/pages";
import { session } from "./src/shared/lib/session";
import { NEXT_PARAM, safeNextPath, withNext } from "./src/shared/lib/redirect";

export async function proxy(request: NextRequest) {
  const { pathname, search } = request.nextUrl;
  const authenticated = await session.isAuthenticated();
  const isAuth = AUTH_PAGES.has(pathname as Page);
  if (isAuth && authenticated) {
    const next = safeNextPath(request.nextUrl.searchParams.get(NEXT_PARAM));
    return NextResponse.redirect(new URL(next, request.url));
  }

  const isProtected = PROTECTED_PAGES.has(pathname as Page);
  if (isProtected) {
    if (!authenticated) {
      // Remember where the user was going (e.g. an invite link) to return after login.
      return NextResponse.redirect(new URL(withNext(PAGES.LOGIN, pathname + search), request.url));
    }
  }

  if (!isAuth && !isProtected) {
    return NextResponse.redirect(new URL(PAGES.LOGIN, request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|.*\\.png$).*)"],
};
