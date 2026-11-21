"use server";

import { revalidateTag, refresh } from "next/cache";
import { apiPath } from "@/src/shared/lib/api-path";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { ActionState } from "@/src/shared/types/action-state";
import {
  ContributionPlan,
  InvestmentGroupInput,
  InvestmentHoldingInput,
  InvestmentPricesInput,
} from "../types/investment";

export type InvestmentActionState = ActionState<Record<string, never>>;

async function send(path: string, method: string, body?: unknown): Promise<InvestmentActionState> {
  try {
    const response = await serverFetch<ApiResponse<unknown> | null>(path, {
      method,
      auth: true,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
    // 204 No Content comes back as null.
    if (response && !response.success) {
      return { status: "error", message: response.message ?? "Error al guardar", errors: {} };
    }
    revalidateTag("investments", {});
    refresh();
    return { status: "success" };
  } catch {
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}

export async function updatePortfolioAction(input: { targetAmountUsd: number; usdRate: number }) {
  return send("/investments/portfolio", "PUT", input);
}

export async function updatePricesAction(input: InvestmentPricesInput) {
  return send("/investments/prices", "PUT", input);
}

export async function saveGroupAction(input: InvestmentGroupInput, id?: string) {
  return id
    ? send(apiPath`/investments/groups/${id}`, "PUT", input)
    : send("/investments/groups", "POST", input);
}

export async function deleteGroupAction(id: string) {
  return send(apiPath`/investments/groups/${id}`, "DELETE");
}

export async function saveHoldingAction(input: InvestmentHoldingInput, id?: string) {
  return id
    ? send(apiPath`/investments/holdings/${id}`, "PUT", input)
    : send("/investments/holdings", "POST", input);
}

export async function deleteHoldingAction(id: string) {
  return send(apiPath`/investments/holdings/${id}`, "DELETE");
}

// A what-if computed by the API (same logic the MCP tool uses); nothing is saved.
export async function simulateContributionAction(
  amount: number,
  currency: "USD" | "ARS",
): Promise<{ status: "success"; plan: ContributionPlan } | { status: "error"; message: string }> {
  try {
    const response = await serverFetch<ApiResponse<ContributionPlan>>("/investments/simulate", {
      method: "POST",
      auth: true,
      body: JSON.stringify({ amount, currency }),
    });
    if (!response?.success || !response.data) {
      return { status: "error", message: response?.message ?? "No se pudo simular" };
    }
    return { status: "success", plan: response.data };
  } catch {
    return { status: "error", message: "Error de conexión" };
  }
}
