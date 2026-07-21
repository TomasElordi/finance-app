"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/src/shared/components/ui/button";

interface YearSelectorProps {
  year: number;
}

export default function YearSelector({ year }: YearSelectorProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const currentYear = new Date().getFullYear();

  function navigate(delta: number) {
    const params = new URLSearchParams(searchParams.toString());
    params.set("bsYear", (year + delta).toString());
    router.push(`?${params.toString()}`);
  }

  return (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="icon" onClick={() => navigate(-1)}>
        <ChevronLeft className="h-4 w-4" />
      </Button>
      <span className="text-sm font-medium w-16 text-center">{year}</span>
      <Button variant="outline" size="icon" onClick={() => navigate(1)} disabled={year >= currentYear}>
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  );
}
