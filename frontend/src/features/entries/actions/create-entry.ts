"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { revalidatePath } from "next/cache";
import { PAGES } from "@/src/shared/lib/pages";
import { CreateEntrySchema } from "@/src/features/entries/types/create-entry-schema";
import { CreateEntryActionState } from "@/src/features/entries/types/create-entry-action-state";
import { Entry } from "@/src/features/entries/types/entry";

export async function createEntryAction(
  _prevState: CreateEntryActionState,
  formData: FormData,
): Promise<CreateEntryActionState> {
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
  // En raw, agregar:

  // Donde construís la date (reemplazá la línea actual):
  const offset = Number(formData.get("timezoneOffset") ?? 0); // minutos, ej: 180 para -03:00
  const sign = offset <= 0 ? "+" : "-";
  const absOffset = Math.abs(offset);
  const hh = String(Math.floor(absOffset / 60)).padStart(2, "0");
  const mm = String(absOffset % 60).padStart(2, "0");
  const date = `${parsed.data.date}T00:00:00.000${sign}${hh}:${mm}`;
  try {
    const body = {
      ...parsed.data,
      date: date,
    };

    const response = await serverFetch<ApiResponse<Entry>>("/entry", {
      method: "POST",
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
