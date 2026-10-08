"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/src/shared/components/ui/button";

interface EntriesPaginationProps {
  page: number;
  totalPages: number;
  totalCount: number;
}

export default function EntriesPagination({
  page,
  totalPages,
  totalCount,
}: EntriesPaginationProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  function goTo(target: number) {
    const params = new URLSearchParams(searchParams.toString());
    params.set("page", target.toString());
    router.push(`?${params.toString()}`);
  }

  return (
    <div className="flex items-center justify-between">
      <span className="text-sm text-muted-foreground">
        {totalCount} asientos
      </span>
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="icon"
          onClick={() => goTo(page - 1)}
          disabled={page <= 1}
        >
          <ChevronLeft className="h-4 w-4" />
        </Button>
        <span className="text-sm font-medium tabular-nums">
          Página {page} de {Math.max(totalPages, 1)}
        </span>
        <Button
          variant="outline"
          size="icon"
          onClick={() => goTo(page + 1)}
          disabled={page >= totalPages}
        >
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
