import { Suspense } from "react";
import CardExpensesSection from "@/src/features/card-expenses/components/card-expenses-section";
import CardExpensesSkeleton from "@/src/features/card-expenses/components/card-expenses-skeleton";

interface CardExpensesPageProps {
  searchParams: Promise<{ year?: string; month?: string }>;
}

export default function CardExpensesPage({ searchParams }: CardExpensesPageProps) {
  return (
    <Suspense fallback={<CardExpensesSkeleton />}>
      <CardExpensesSectionWrapper searchParams={searchParams} />
    </Suspense>
  );
}

async function CardExpensesSectionWrapper({
  searchParams,
}: {
  searchParams: Promise<{ year?: string; month?: string }>;
}) {
  const params = await searchParams;
  const now = new Date();
  const year = params.year ? parseInt(params.year) : now.getFullYear();
  const month = params.month ? parseInt(params.month) : now.getMonth() + 1;

  return <CardExpensesSection year={year} month={month} />;
}
