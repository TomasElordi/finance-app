"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/src/shared/components/ui/button";

const MONTH_NAMES = [
  "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
  "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre",
];

interface PeriodSelectorProps {
  year: number;
  month: number;
}

export default function PeriodSelector({ year, month }: PeriodSelectorProps) {
  const router = useRouter();
  const searchParams = useSearchParams();

  const now = new Date();
  const isCurrentPeriod = year === now.getFullYear() && month === now.getMonth() + 1;

  function navigate(delta: number) {
    const date = new Date(year, month - 1 + delta, 1);
    const params = new URLSearchParams(searchParams.toString());
    params.set("year", date.getFullYear().toString());
    params.set("month", (date.getMonth() + 1).toString());
    router.push(`?${params.toString()}`);
  }

  return (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="icon" onClick={() => navigate(-1)}>
        <ChevronLeft className="h-4 w-4" />
      </Button>
      <span className="text-sm font-medium w-36 text-center">
        {MONTH_NAMES[month - 1]} {year}
      </span>
      <Button
        variant="outline"
        size="icon"
        onClick={() => navigate(1)}
        disabled={isCurrentPeriod}
      >
        <ChevronRight className="h-4 w-4" />
      </Button>
    </div>
  );
}
