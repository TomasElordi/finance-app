import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { SharedGroupDetail } from "../types/shared";

async function fetchSharedGroup(token: string, groupId: string): Promise<SharedGroupDetail> {
  "use cache";
  cacheTag("shared-expenses");
  return fetchApiData<SharedGroupDetail>(`/shared-groups/${groupId}`, token);
}

export async function getSharedGroup(groupId: string): Promise<SharedGroupDetail | null> {
  const token = await session.getAccessToken();
  if (!token) return null;
  return fetchSharedGroup(token, groupId);
}
