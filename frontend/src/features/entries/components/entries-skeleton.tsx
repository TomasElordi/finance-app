import { Skeleton } from "@/src/shared/components/ui/skeleton";

function EntryCardSkeleton() {
  return (
    <div className="flex flex-col gap-3 rounded-lg border px-4 py-3">
      <div className="flex items-start justify-between gap-2">
        <div className="flex flex-col gap-1.5">
          <Skeleton className="h-4 w-48" />
          <Skeleton className="h-3 w-24" />
        </div>
        <div className="flex items-center gap-1 shrink-0">
          <Skeleton className="h-8 w-8 rounded-md" />
          <Skeleton className="h-8 w-8 rounded-md" />
        </div>
      </div>
      <div className="flex flex-col gap-1.5 pl-1">
        <Skeleton className="h-3 w-full" />
        <Skeleton className="h-3 w-4/5" />
        <Skeleton className="h-3 w-3/5" />
      </div>
    </div>
  );
}

export default function EntriesSkeleton() {
  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-28" />
        <Skeleton className="h-9 w-32 rounded-md" />
      </div>
      <div className="flex flex-col gap-3">
        {Array.from({ length: 5 }).map((_, i) => (
          <EntryCardSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
