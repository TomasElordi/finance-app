"use server";

import { apiPath } from "@/src/shared/lib/api-path";
import { ActionResult, SharedSettlement, SharedSettlementInput } from "../types/shared";
import { runSharedRequest } from "./run";

export async function createSettlementAction(
  groupId: string,
  input: SharedSettlementInput,
): Promise<ActionResult<SharedSettlement>> {
  return runSharedRequest<SharedSettlement>(apiPath`/${groupId}/settlements`, "POST", input);
}

export async function deleteSettlementAction(groupId: string, settlementId: string): Promise<ActionResult> {
  return runSharedRequest(apiPath`/${groupId}/settlements/${settlementId}`, "DELETE");
}
