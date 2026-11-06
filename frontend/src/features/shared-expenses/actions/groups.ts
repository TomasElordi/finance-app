"use server";

import { ActionResult, SharedGroupDetail } from "../types/shared";
import { runSharedRequest } from "./run";

export async function createGroupAction(name: string): Promise<ActionResult<SharedGroupDetail>> {
  return runSharedRequest<SharedGroupDetail>("", "POST", { name });
}

export async function joinGroupAction(code: string): Promise<ActionResult<SharedGroupDetail>> {
  return runSharedRequest<SharedGroupDetail>("/join", "POST", { code });
}

export async function renameGroupAction(groupId: string, name: string): Promise<ActionResult<SharedGroupDetail>> {
  return runSharedRequest<SharedGroupDetail>(`/${groupId}`, "PUT", { name });
}

export async function regenerateInviteCodeAction(groupId: string): Promise<ActionResult<SharedGroupDetail>> {
  return runSharedRequest<SharedGroupDetail>(`/${groupId}/invite-code`, "POST");
}

export async function leaveGroupAction(groupId: string): Promise<ActionResult> {
  return runSharedRequest(`/${groupId}/members/me`, "DELETE");
}
