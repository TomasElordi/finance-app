import { revalidateTag, refresh } from "next/cache";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionResult } from "../types/shared";

// Shared plumbing for the shared-expenses server actions: calls the API, maps
// failures to a message and invalidates the cached shared data on success.
export async function runSharedRequest<T>(
  path: string,
  method: "POST" | "PUT" | "DELETE",
  body?: unknown,
): Promise<ActionResult<T>> {
  try {
    const response = await serverFetch<ApiResponse<T> | null>(`/shared-groups${path}`, {
      method,
      auth: true,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
    // 204 No Content comes back as null.
    if (response && !response.success) {
      return { status: "error", message: response.message ?? "Ocurrió un error" };
    }
    revalidateTag("shared-expenses", {});
    refresh();
    return { status: "success", data: (response?.data ?? null) as T };
  } catch {
    return { status: "error", message: "Error de conexión" };
  }
}
