"use server";

import { ActionResult, SharedExpense, SharedExpenseInput } from "../types/shared";
import { runSharedRequest } from "./run";

// Creates the expense when expenseId is undefined, updates it otherwise.
export async function saveExpenseAction(
  groupId: string,
  input: SharedExpenseInput,
  expenseId?: string,
): Promise<ActionResult<SharedExpense>> {
  return expenseId
    ? runSharedRequest<SharedExpense>(`/${groupId}/expenses/${expenseId}`, "PUT", input)
    : runSharedRequest<SharedExpense>(`/${groupId}/expenses`, "POST", input);
}

export async function deleteExpenseAction(groupId: string, expenseId: string): Promise<ActionResult> {
  return runSharedRequest(`/${groupId}/expenses/${expenseId}`, "DELETE");
}
