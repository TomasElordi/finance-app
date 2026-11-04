import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { CardUpcoming } from "../types/card";

async function fetchCardUpcoming(token: string, year: number, month: number): Promise<CardUpcoming> {
  "use cache";
  cacheTag("card-items");
  return fetchApiData<CardUpcoming>(`/card/upcoming?year=${year}&month=${month}`, token);
}

export async function getCardUpcoming(year: number, month: number): Promise<CardUpcoming> {
  const token = await session.getAccessToken();
  if (!token) return { total: 0, people: [], months: [] };
  return fetchCardUpcoming(token, year, month);
}
