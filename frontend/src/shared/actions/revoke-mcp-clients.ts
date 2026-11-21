"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";

export type RevokeMcpClientsResult =
  | { status: "success"; clients: number }
  | { status: "error"; message: string };

// Disconnects every MCP client (Claude, etc.) connected to the user's account.
export async function revokeMcpClientsAction(): Promise<RevokeMcpClientsResult> {
  try {
    const response = await serverFetch<ApiResponse<{ clients: number }>>(
      "/auth/mcp/revoke",
      { method: "POST" },
    );
    if (!response?.success) {
      return { status: "error", message: "No se pudieron desconectar los clientes." };
    }
    return { status: "success", clients: response.data.clients };
  } catch (error) {
    console.log("revoke MCP clients failed", error);
    return { status: "error", message: "Error de conexión" };
  }
}
