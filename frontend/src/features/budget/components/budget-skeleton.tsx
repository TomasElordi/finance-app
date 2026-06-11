import { Skeleton } from "@/src/shared/components/ui/skeleton";

export default function BudgetSkeleton() {
  return (
    <div className="p-6 flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-36" />
        <Skeleton className="h-9 w-48 rounded-md" />
      </div>
      <div className="flex flex-col gap-4">
        <Skeleton className="h-6 w-44" />
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full rounded-lg" />
        ))}
        <div className="flex justify-end">
          <Skeleton className="h-9 w-44 rounded-md" />
        </div>
      </div>
    </div>
  );
}
