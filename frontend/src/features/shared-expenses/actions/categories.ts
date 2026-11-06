"use server";

import { ActionResult, SharedCategory } from "../types/shared";
import { runSharedRequest } from "./run";

export async function createCategoryAction(groupId: string, name: string): Promise<ActionResult<SharedCategory>> {
  return runSharedRequest<SharedCategory>(`/${groupId}/categories`, "POST", { name });
}

export async function deleteCategoryAction(groupId: string, categoryId: string): Promise<ActionResult> {
  return runSharedRequest(`/${groupId}/categories/${categoryId}`, "DELETE");
}
