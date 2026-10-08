import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { PaginatedEntries } from "../types/entry";
import { session } from "@/src/shared/lib/session";

export const ENTRIES_PAGE_SIZE = 20;

function emptyPage(page: number, pageSize: number): PaginatedEntries {
  return { entries: [], page, pageSize, totalCount: 0, totalPages: 0 };
}

async function fetchEntries(
  token: string,
  page: number,
  pageSize: number,
): Promise<PaginatedEntries> {
  "use cache";
  cacheTag("entries");
  try {
    const response = await serverFetch<ApiResponse<PaginatedEntries>>(
      `/entry?page=${page}&pageSize=${pageSize}`,
      { auth: false, headers: { Authorization: `Bearer ${token}` } },
    );
    return response?.success ? response.data : emptyPage(page, pageSize);
  } catch {
    return emptyPage(page, pageSize);
  }
}

export async function getEntries(
  page = 1,
  pageSize = ENTRIES_PAGE_SIZE,
): Promise<PaginatedEntries> {
  const token = await session.getAccessToken();
  if (!token) return emptyPage(page, pageSize);
  return fetchEntries(token, page, pageSize);
}
