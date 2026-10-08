import { Suspense } from "react";
import ExpensesSection from "@/src/features/expenses/components/expenses-section";
import ExpensesSkeleton from "@/src/features/expenses/components/expenses-skeleton";

type ExpensesSearchParams = Promise<{ year?: string; month?: string; account?: string }>;

const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export default function ExpensesPage({ searchParams }: { searchParams: ExpensesSearchParams }) {
  return (
    <Suspense fallback={<ExpensesSkeleton />}>
      <ExpensesSectionWrapper searchParams={searchParams} />
    </Suspense>
  );
}

async function ExpensesSectionWrapper({ searchParams }: { searchParams: ExpensesSearchParams }) {
  const params = await searchParams;
  const now = new Date();
  const parsedYear = params.year ? parseInt(params.year) : NaN;
  const parsedMonth = params.month ? parseInt(params.month) : NaN;
  const year = parsedYear >= 2000 && parsedYear <= 2100 ? parsedYear : now.getFullYear();
  const month = parsedMonth >= 1 && parsedMonth <= 12 ? parsedMonth : now.getMonth() + 1;
  const accountId = params.account && UUID_PATTERN.test(params.account) ? params.account : undefined;

  return <ExpensesSection year={year} month={month} accountId={accountId} />;
}
