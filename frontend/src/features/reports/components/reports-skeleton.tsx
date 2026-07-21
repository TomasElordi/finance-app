import { Skeleton } from "@/src/shared/components/ui/skeleton";

export default function ReportsSkeleton() {
  return (
    <div className="p-6 flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-40" />
        <Skeleton className="h-9 w-32 rounded-md" />
      </div>
      <Skeleton className="h-10 w-64 rounded-lg" />
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <Skeleton key={i} className="h-20 w-full rounded-lg" />
        ))}
      </div>
      <div className="grid gap-6 md:grid-cols-2">
        {Array.from({ length: 2 }).map((_, col) => (
          <div key={col} className="flex flex-col gap-2">
            <Skeleton className="h-5 w-24" />
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-12 w-full rounded-lg" />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
