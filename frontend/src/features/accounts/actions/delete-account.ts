"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { revalidatePath } from "next/cache";
import { PAGES } from "@/src/shared/lib/pages";
import { ActionState } from "@/src/shared/types/action-state";

export async function deleteAccountAction(
  _prevState: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const id = formData.get("id") as string;

  if (!id) {
    return { status: "error", message: "ID requerido", errors: {} };
  }

  try {
    const response = await serverFetch<ApiResponse<null>>(`/account?Id=${id}`, {
      method: "DELETE",
      auth: true,
    });

    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidatePath(PAGES.ACCOUNTS);
    return { status: "success" };
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
