import { cacheTag } from "next/dist/server/use-cache/cache-tag";
import { fetchApiData } from "@/src/shared/lib/api";
import { session } from "@/src/shared/lib/session";
import { Person } from "../types/card";

async function fetchPeople(token: string): Promise<Person[]> {
  "use cache";
  cacheTag("people");
  return fetchApiData<Person[]>("/card/people", token);
}

export async function getPeople(): Promise<Person[]> {
  const token = await session.getAccessToken();
  if (!token) return [];
  return fetchPeople(token);
}
