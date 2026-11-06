import { Suspense } from "react";
import SharedExpensesSection from "@/src/features/shared-expenses/components/shared-expenses-section";
import SharedExpensesSkeleton from "@/src/features/shared-expenses/components/shared-expenses-skeleton";

type SearchParams = Promise<{ group?: string; year?: string; month?: string; join?: string }>;

export default function SharedExpensesPage({ searchParams }: { searchParams: SearchParams }) {
  return (
    <Suspense fallback={<SharedExpensesSkeleton />}>
      <SharedExpensesSectionWrapper searchParams={searchParams} />
    </Suspense>
  );
}

async function SharedExpensesSectionWrapper({ searchParams }: { searchParams: SearchParams }) {
  const params = await searchParams;
  const now = new Date();
  const year = params.year ? parseInt(params.year) : now.getFullYear();
  const month = params.month ? parseInt(params.month) : now.getMonth() + 1;

  return (
    <SharedExpensesSection
      groupId={params.group}
      year={year}
      month={month}
      joinCode={params.join}
    />
  );
}
