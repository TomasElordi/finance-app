"use server";

import { apiPath } from "@/src/shared/lib/api-path";
import { revalidateTag, refresh } from "next/cache";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";

// With `from`, a recurring item stops being charged from that month on instead of being deleted.
export async function deleteCardItemAction(
  id: string,
  from?: { year: number; month: number },
): Promise<ActionState<Record<string, never>>> {
  try {
    const path = from
      ? apiPath`/card/items/${id}?fromYear=${from.year}&fromMonth=${from.month}`
      : apiPath`/card/items/${id}`;
    const response = await serverFetch<ApiResponse<null> | null>(path, {
      method: "DELETE",
      auth: true,
    });
    // 204 No Content comes back as null.
    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }
    revalidateTag("card-items", {});
    refresh();
    return { status: "success" };
  } catch {
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
