import { Suspense } from "react";
import BudgetSection from "@/src/features/budget/components/budget-section";
import BudgetSkeleton from "@/src/features/budget/components/budget-skeleton";

interface BudgetPageProps {
  searchParams: Promise<{ year?: string; month?: string }>;
}

export default function BudgetPage({ searchParams }: BudgetPageProps) {
  return (
    <Suspense fallback={<BudgetSkeleton />}>
      <BudgetSectionWrapper searchParams={searchParams} />
    </Suspense>
  );
}

async function BudgetSectionWrapper({
  searchParams,
}: {
  searchParams: Promise<{ year?: string; month?: string }>;
}) {
  const params = await searchParams;
  const now = new Date();
  const year = params.year ? parseInt(params.year) : now.getFullYear();
  const month = params.month ? parseInt(params.month) : now.getMonth() + 1;

  return <BudgetSection year={year} month={month} />;
}
