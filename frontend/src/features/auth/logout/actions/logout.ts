"use server";

import { session } from "@/src/shared/lib/session";
import { PAGES } from "@/src/shared/lib/pages";
import { serverFetch, sessionCacheTag } from "@/src/shared/lib/api";
import { redirect } from "next/navigation";
import { updateTag } from "next/cache";

export async function logoutAction() {
  const token = await session.getAccessToken();
  if (token) {
    // Revoke the token in the API, so a copy of it stops working too, and drop the
    // data cached with it. A failure here still logs the user out locally.
    try {
      await serverFetch("/auth/logout", { method: "POST" });
    } catch (error) {
      console.log("logout: API revoke failed", error);
    }
    updateTag(sessionCacheTag(token));
  }
  await session.clear();
  redirect(PAGES.LOGIN);
}
