import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { EMPTY_PORTFOLIO, InvestmentPortfolio } from "../types/investment";

async function fetchInvestments(token: string): Promise<InvestmentPortfolio> {
  "use cache";
  cacheTag("investments");
  return fetchApiData<InvestmentPortfolio>("/investments", token);
}

export async function getInvestments(): Promise<InvestmentPortfolio> {
  const token = await session.getAccessToken();
  if (!token) return EMPTY_PORTFOLIO;
  return fetchInvestments(token);
}
