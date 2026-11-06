import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { SharedGroupSummary } from "../types/shared";

// Shared data changes when any member edits it. revalidateTag is global to the
// server, so a member's change also invalidates the other members' cached copies.
async function fetchSharedGroups(token: string): Promise<SharedGroupSummary[]> {
  "use cache";
  cacheTag("shared-expenses");
  return fetchApiData<SharedGroupSummary[]>("/shared-groups", token);
}

export async function getSharedGroups(): Promise<SharedGroupSummary[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchSharedGroups(token);
}
