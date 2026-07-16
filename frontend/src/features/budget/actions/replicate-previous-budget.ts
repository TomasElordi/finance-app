"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";
import { Budget } from "../types/budget";
import { revalidateTag, refresh } from "next/cache";

export async function replicatePreviousBudgetAction(
  year: number,
  month: number,
): Promise<ActionState> {
  try {
    const response = await serverFetch<ApiResponse<Budget[]>>(
      `/budget/replicate-previous?year=${year}&month=${month}`,
      { method: "POST", auth: true },
    );

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
