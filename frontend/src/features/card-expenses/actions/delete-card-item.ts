"use server";

import { revalidateTag, refresh } from "next/cache";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";

export async function deleteCardItemAction(id: string): Promise<ActionState<Record<string, never>>> {
  try {
    const response = await serverFetch<ApiResponse<null> | null>(`/card/items/${id}`, {
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
