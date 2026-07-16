"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";
import { Entry } from "@/src/features/entries/types/entry";
import { revalidateTag, refresh } from "next/cache";

export async function bulkCreateEntriesAction(
  _prevState: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const raw = String(formData.get("entriesJson") ?? "");

  let entries: unknown;
  try {
    entries = JSON.parse(raw);
  } catch {
    return { status: "error", message: "El JSON no es válido.", errors: {} };
  }

  if (!Array.isArray(entries) || entries.length === 0) {
    return {
      status: "error",
      message: "El JSON debe ser un array con al menos un asiento.",
      errors: {},
    };
  }

  try {
    const response = await serverFetch<ApiResponse<Entry[]>>("/entry/bulk", {
      method: "POST",
      auth: true,
      body: JSON.stringify({ entries }),
    });

    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidateTag("entries", {});
    revalidateTag("accounts", {});
    refresh();
    return { status: "success" };
  } catch {
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
