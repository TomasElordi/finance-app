"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { revalidateTag, refresh } from "next/cache";
import { ActionState } from "@/src/shared/types/action-state";
import { Entry } from "@/src/features/entries/types/entry";

export async function closeMonthAction(
  _prevState: ActionState,
  _formData: FormData,
): Promise<ActionState> {
  try {
    const response = await serverFetch<ApiResponse<Entry>>("/entry/close-month", {
      method: "POST",
      auth: true,
    });

    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidateTag("entries", {});
    revalidateTag("accounts", {});
    revalidateTag("reports", {});
    refresh();
    return { status: "success" };
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
