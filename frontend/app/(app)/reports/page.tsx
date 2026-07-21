import { Suspense } from "react";
import ReportsSection from "@/src/features/reports/components/reports-section";
import ReportsSkeleton from "@/src/features/reports/components/reports-skeleton";

interface ReportsPageProps {
  searchParams: Promise<{ tab?: string; year?: string; month?: string; bsYear?: string }>;
}

export default function ReportsPage({ searchParams }: ReportsPageProps) {
  return (
    <Suspense fallback={<ReportsSkeleton />}>
      <ReportsSectionWrapper searchParams={searchParams} />
    </Suspense>
  );
}

async function ReportsSectionWrapper({
  searchParams,
}: {
  searchParams: Promise<{ tab?: string; year?: string; month?: string; bsYear?: string }>;
}) {
  const params = await searchParams;
  const now = new Date();
  const year = params.year ? parseInt(params.year) : now.getFullYear();
  const month = params.month ? parseInt(params.month) : now.getMonth() + 1;
  const bsYear = params.bsYear ? parseInt(params.bsYear) : now.getFullYear();
  const tab = params.tab === "balance" ? "balance" : "income";

  return <ReportsSection tab={tab} year={year} month={month} bsYear={bsYear} />;
}
