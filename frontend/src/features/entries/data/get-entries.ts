import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { Entry } from "../types/entry";
import { session } from "@/src/shared/lib/session";

async function fetchEntries(token: string): Promise<Entry[]> {
  "use cache";
  cacheTag("entries");
  try {
    const response = await serverFetch<ApiResponse<{ entries: Entry[] }>>(
      "/entry",
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data.entries : [];
  } catch {
    return [];
  }
}

export async function getEntries(): Promise<Entry[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchEntries(token);
}
