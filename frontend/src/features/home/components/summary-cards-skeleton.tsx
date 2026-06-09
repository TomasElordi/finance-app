import { Skeleton } from "@/src/shared/components/ui/skeleton";
import { Card, CardContent, CardHeader } from "@/src/shared/components/ui/card";

export default function SummaryCardsSkeleton() {
  return (
    <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
      {Array.from({ length: 4 }).map((_, i) => (
        <Card key={i}>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="size-4 rounded-full" />
          </CardHeader>
          <CardContent>
            <Skeleton className="h-7 w-32" />
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
