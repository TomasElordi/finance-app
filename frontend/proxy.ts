import { NextRequest, NextResponse } from "next/server";
import {
  AUTH_PAGES,
  Page,
  PAGES,
  PROTECTED_PAGES,
} from "./src/shared/lib/pages";
import { session } from "./src/shared/lib/session";

export async function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  const authenticated = await session.isAuthenticated();
  const isAuth = AUTH_PAGES.has(pathname as Page);
  if (isAuth && authenticated) {
    return NextResponse.redirect(new URL(PAGES.HOME, request.url));
  }

  const isProtected = PROTECTED_PAGES.has(pathname as Page);

  if (isProtected) {
    if (!authenticated) {
      return NextResponse.redirect(new URL(PAGES.LOGIN, request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!api|_next/static|_next/image|.*\\.png$).*)"],
};
