import { Skeleton } from "@/src/shared/components/ui/skeleton";

export default function RecentEntriesSkeleton() {
  return (
    <div className="flex flex-col gap-3">
      <Skeleton className="h-4 w-32" />
      <div className="flex flex-col gap-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <div key={i} className="flex flex-col gap-2 rounded-lg border px-4 py-3">
            <div className="flex items-start justify-between">
              <div className="flex flex-col gap-1">
                <Skeleton className="h-4 w-40" />
                <Skeleton className="h-3 w-24" />
              </div>
              <div className="flex gap-1">
                <Skeleton className="size-8 rounded-md" />
                <Skeleton className="size-8 rounded-md" />
              </div>
            </div>
            <Skeleton className="h-3 w-full" />
            <Skeleton className="h-3 w-3/4" />
          </div>
        ))}
      </div>
    </div>
  );
}
