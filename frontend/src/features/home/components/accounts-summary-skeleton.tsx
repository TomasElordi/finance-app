import { Skeleton } from "@/src/shared/components/ui/skeleton";

export default function AccountsSummarySkeleton() {
  return (
    <div className="flex flex-col gap-3">
      <Skeleton className="h-4 w-20" />
      <div className="flex flex-col gap-4">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="flex flex-col gap-1.5">
            <Skeleton className="h-5 w-28 rounded-full" />
            <div className="flex flex-col gap-1">
              {Array.from({ length: 2 }).map((_, j) => (
                <div key={j} className="flex items-center justify-between rounded-md px-3 py-2 border">
                  <Skeleton className="h-4 w-28" />
                  <Skeleton className="h-4 w-20" />
                </div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
