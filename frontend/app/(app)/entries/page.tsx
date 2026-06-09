import { Suspense } from "react";
import EntriesSection from "@/src/features/entries/components/entries-section";
import EntriesSkeleton from "@/src/features/entries/components/entries-skeleton";

export default function EntriesPage() {
  return (
    <Suspense fallback={<EntriesSkeleton />}>
      <EntriesSection />
    </Suspense>
  );
}
