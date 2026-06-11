"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";
import { Budget } from "../types/budget";
import { revalidateTag, refresh } from "next/cache";

export interface UpsertBudgetItem {
  accountId: string;
  year: number;
  month: number;
  amount: number;
}

export type UpsertBudgetsActionState = ActionState<Record<string, never>>;

export async function upsertBudgetsAction(items: UpsertBudgetItem[]): Promise<UpsertBudgetsActionState> {
  try {
    const response = await serverFetch<ApiResponse<Budget[]>>("/budget/bulk", {
      method: "POST",
      auth: true,
      body: JSON.stringify(items),
    });

    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidateTag("budgets", {});
    revalidateTag("budget-summary", {});
    refresh();
    return { status: "success" };
  } catch {
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
