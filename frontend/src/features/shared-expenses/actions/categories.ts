"use server";

import { apiPath } from "@/src/shared/lib/api-path";
import { ActionResult, SharedCategory } from "../types/shared";
import { runSharedRequest } from "./run";

export async function createCategoryAction(groupId: string, name: string): Promise<ActionResult<SharedCategory>> {
  return runSharedRequest<SharedCategory>(apiPath`/${groupId}/categories`, "POST", { name });
}

export async function deleteCategoryAction(groupId: string, categoryId: string): Promise<ActionResult> {
  return runSharedRequest(apiPath`/${groupId}/categories/${categoryId}`, "DELETE");
}
