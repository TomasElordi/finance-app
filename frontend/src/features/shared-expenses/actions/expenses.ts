"use server";

import { apiPath } from "@/src/shared/lib/api-path";
import { ActionResult, SharedExpense, SharedExpenseInput } from "../types/shared";
import { runSharedRequest } from "./run";

// Creates the expense when expenseId is undefined, updates it otherwise.
export async function saveExpenseAction(
  groupId: string,
  input: SharedExpenseInput,
  expenseId?: string,
): Promise<ActionResult<SharedExpense>> {
  return expenseId
    ? runSharedRequest<SharedExpense>(apiPath`/${groupId}/expenses/${expenseId}`, "PUT", input)
    : runSharedRequest<SharedExpense>(apiPath`/${groupId}/expenses`, "POST", input);
}

export async function deleteExpenseAction(groupId: string, expenseId: string): Promise<ActionResult> {
  return runSharedRequest(apiPath`/${groupId}/expenses/${expenseId}`, "DELETE");
}
