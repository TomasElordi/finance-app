import { Skeleton } from "@/src/shared/components/ui/skeleton";

function AccountCardSkeleton() {
  return (
    <div className="flex items-center justify-between rounded-lg border bg-card px-4 py-3">
      <div className="flex flex-col gap-1.5">
        <Skeleton className="h-4 w-32" />
        <Skeleton className="h-4 w-16 rounded-full" />
      </div>
      <div className="flex items-center gap-3">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-8 w-8 rounded-md" />
      </div>
    </div>
  );
}

export default function AccountsSkeleton() {
  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-28" />
        <Skeleton className="h-9 w-32 rounded-md" />
      </div>
      <div className="flex flex-col gap-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <AccountCardSkeleton key={i} />
        ))}
      </div>
    </div>
  );
}
