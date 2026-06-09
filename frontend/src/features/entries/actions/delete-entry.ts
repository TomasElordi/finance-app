"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { revalidateTag, refresh } from "next/cache";
import { ActionState } from "@/src/shared/types/action-state";

export async function deleteEntryAction(
  _prevState: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const id = formData.get("id") as string;

  if (!id) {
    return { status: "error", message: "ID requerido", errors: {} };
  }

  try {
    const response = await serverFetch<ApiResponse<null>>(`/entry/${id}`, {
      method: "DELETE",
      auth: true,
    });

    if (response !== null && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidateTag("entries", {});
    revalidateTag("accounts", {});
    refresh();
    return { status: "success" };
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
