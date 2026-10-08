import { Suspense } from "react";
import EntriesSection from "@/src/features/entries/components/entries-section";
import EntriesSkeleton from "@/src/features/entries/components/entries-skeleton";

interface EntriesPageProps {
  searchParams: Promise<{ page?: string }>;
}

export default function EntriesPage({ searchParams }: EntriesPageProps) {
  return (
    <Suspense fallback={<EntriesSkeleton />}>
      <EntriesSectionWrapper searchParams={searchParams} />
    </Suspense>
  );
}

async function EntriesSectionWrapper({
  searchParams,
}: {
  searchParams: Promise<{ page?: string }>;
}) {
  const params = await searchParams;
  const parsed = params.page ? parseInt(params.page) : 1;
  const page = Number.isNaN(parsed) || parsed < 1 ? 1 : parsed;

  return <EntriesSection page={page} />;
}
