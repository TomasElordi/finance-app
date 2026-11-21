import { Suspense } from "react";
import InvestmentsSection from "@/src/features/investments/components/investments-section";
import InvestmentsSkeleton from "@/src/features/investments/components/investments-skeleton";

export default function InvestmentsPage() {
  return (
    <Suspense fallback={<InvestmentsSkeleton />}>
      <InvestmentsSection />
    </Suspense>
  );
}
