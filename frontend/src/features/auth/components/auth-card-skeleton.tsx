import {
  Card,
  CardContent,
  CardHeader,
} from "@/src/shared/components/ui/card";
import { Skeleton } from "@/src/shared/components/ui/skeleton";

export default function AuthCardSkeleton({ fields = 2 }: { fields?: number }) {
  return (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <div className="flex items-start justify-between">
          <div className="flex flex-col gap-2">
            <Skeleton className="h-5 w-44" />
            <Skeleton className="h-4 w-64" />
          </div>
          <Skeleton className="h-8 w-16 rounded-md" />
        </div>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col gap-6">
          {Array.from({ length: fields }).map((_, i) => (
            <div key={i} className="flex flex-col gap-2">
              <Skeleton className="h-4 w-16" />
              <Skeleton className="h-9 w-full rounded-md" />
            </div>
          ))}
        </div>
        <Skeleton className="h-px w-full my-4" />
        <Skeleton className="h-9 w-full rounded-md" />
      </CardContent>
    </Card>
  );
}
