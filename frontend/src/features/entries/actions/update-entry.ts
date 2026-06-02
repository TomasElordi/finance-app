"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { revalidatePath } from "next/cache";
import { PAGES } from "@/src/shared/lib/pages";
import { CreateEntrySchema } from "@/src/features/entries/types/create-entry-schema";
import { UpdateEntryActionState } from "@/src/features/entries/types/update-entry-action-state";
import { Entry } from "@/src/features/entries/types/entry";

export async function updateEntryAction(
  _prevState: UpdateEntryActionState,
  formData: FormData,
): Promise<UpdateEntryActionState> {
  const id = formData.get("id") as string;

  if (!id) {
    return { status: "error", message: "ID requerido", errors: {} };
  }

  const entryLines = [];
  let i = 0;
  while (formData.get(`entryLines.${i}.accountId`) !== null) {
    entryLines.push({
      accountId: formData.get(`entryLines.${i}.accountId`),
      amount: formData.get(`entryLines.${i}.amount`),
      type: formData.get(`entryLines.${i}.type`),
    });
    i++;
  }

  const raw = {
    title: formData.get("title"),
    description: formData.get("description") || undefined,
    date: formData.get("date"),
    entryLines,
  };

  const parsed = CreateEntrySchema.safeParse(raw);
  if (!parsed.success) {
    const { fieldErrors } = parsed.error.flatten();
    return {
      status: "error",
      message: "Datos inválidos.",
      errors: {
        title: fieldErrors.title?.[0],
        description: fieldErrors.description?.[0],
        date: fieldErrors.date?.[0],
        entryLines: fieldErrors.entryLines?.[0],
      },
    };
  }

  try {
    const body = {
      ...parsed.data,
      date: new Date(parsed.data.date).toISOString(),
    };

    const response = await serverFetch<ApiResponse<Entry>>(`/entry/${id}`, {
      method: "PUT",
      auth: true,
      body: JSON.stringify(body),
    });

    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidatePath(PAGES.ENTRIES);
    return { status: "success" };
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
