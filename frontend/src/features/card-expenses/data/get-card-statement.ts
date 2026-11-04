import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { CardStatement } from "../types/card";

async function fetchCardStatement(token: string, year: number, month: number): Promise<CardStatement> {
  "use cache";
  cacheTag("card-items");
  return fetchApiData<CardStatement>(`/card/statement?year=${year}&month=${month}`, token);
}

export async function getCardStatement(year: number, month: number): Promise<CardStatement> {
  const token = await session.getAccessToken();
  if (!token) return { year, month, items: [], people: [], total: 0 };
  return fetchCardStatement(token, year, month);
}
