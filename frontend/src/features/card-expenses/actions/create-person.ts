"use server";

import { revalidateTag } from "next/cache";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { Person } from "../types/card";

export type CreatePersonResult =
  | { status: "success"; person: Person }
  | { status: "error"; message: string };

export async function createPersonAction(name: string): Promise<CreatePersonResult> {
  try {
    const response = await serverFetch<ApiResponse<Person>>("/card/people", {
      method: "POST",
      auth: true,
      body: JSON.stringify({ name }),
    });
    if (!response?.success) {
      return { status: "error", message: response?.message ?? "Error al crear la persona" };
    }
    revalidateTag("people", {});
    return { status: "success", person: response.data };
  } catch {
    return { status: "error", message: "Error de conexión" };
  }
}
