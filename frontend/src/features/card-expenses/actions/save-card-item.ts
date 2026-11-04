"use server";

import { revalidateTag, refresh } from "next/cache";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";
import { CardItem, CardItemInput } from "../types/card";

export type SaveCardItemActionState = ActionState<Record<string, never>>;

// Creates the item when id is undefined, updates it otherwise.
export async function saveCardItemAction(
  input: CardItemInput,
  id?: string,
): Promise<SaveCardItemActionState> {
  try {
    const response = await serverFetch<ApiResponse<CardItem>>(
      id ? `/card/items/${id}` : "/card/items",
      { method: id ? "PUT" : "POST", auth: true, body: JSON.stringify(input) },
    );

    if (!response?.success) {
      return { status: "error", message: response?.message ?? "Error al guardar", errors: {} };
    }

    revalidateTag("card-items", {});
    refresh();
    return { status: "success" };
  } catch {
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
