import { revalidateTag } from "next/cache";

// Every cached view whose data is derived from entries
export function revalidateEntryDependents() {
  revalidateTag("entries", {});
  revalidateTag("accounts", {});
  revalidateTag("reports", {});
  revalidateTag("budget-summary", {});
}
