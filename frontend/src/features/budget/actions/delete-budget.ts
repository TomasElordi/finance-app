"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { revalidateTag, refresh } from "next/cache";

export async function deleteBudgetAction(id: string): Promise<void> {
  await serverFetch(`/budget/${id}`, { method: "DELETE", auth: true });
  revalidateTag("budgets", {});
  revalidateTag("budget-summary", {});
  refresh();
}
