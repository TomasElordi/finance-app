"use server";

import { session } from "@/src/shared/lib/session";
import { PAGES } from "@/src/shared/lib/pages";
import { redirect } from "next/navigation";

export async function logoutAction() {
  await session.clear();
  redirect(PAGES.LOGIN);
}
