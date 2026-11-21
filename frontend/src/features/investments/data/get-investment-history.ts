import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { InvestmentSnapshot } from "../types/investment";

async function fetchHistory(token: string): Promise<InvestmentSnapshot[]> {
  "use cache";
  cacheTag("investments");
  return fetchApiData<InvestmentSnapshot[]>("/investments/history", token);
}

// The whole history; the chart filters the range client-side (one row per day is small).
export async function getInvestmentHistory(): Promise<InvestmentSnapshot[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchHistory(token);
}
