"use server";

import { apiPath } from "@/src/shared/lib/api-path";
import { serverFetch } from "@/src/shared/lib/api";
import { revalidateTag, refresh } from "next/cache";

export async function deleteBudgetAction(id: string): Promise<void> {
  await serverFetch(apiPath`/budget/${id}`, { method: "DELETE", auth: true });
  revalidateTag("budgets", {});
  revalidateTag("budget-summary", {});
  refresh();
}
