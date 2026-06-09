import { Suspense } from "react";
import SummaryCardsSection from "@/src/features/home/components/summary-cards-section";
import RecentEntriesSection from "@/src/features/home/components/recent-entries-section";
import AccountsSummarySection from "@/src/features/home/components/accounts-summary-section";
import SummaryCardsSkeleton from "@/src/features/home/components/summary-cards-skeleton";
import RecentEntriesSkeleton from "@/src/features/home/components/recent-entries-skeleton";
import AccountsSummarySkeleton from "@/src/features/home/components/accounts-summary-skeleton";

export default function HomePage() {
  return (
    <div className="flex flex-col gap-6 p-4 md:p-6">
      <Suspense fallback={<SummaryCardsSkeleton />}>
        <SummaryCardsSection />
      </Suspense>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Suspense fallback={<RecentEntriesSkeleton />}>
          <RecentEntriesSection />
        </Suspense>

        <Suspense fallback={<AccountsSummarySkeleton />}>
          <AccountsSummarySection />
        </Suspense>
      </div>
    </div>
  );
}
