"use client";

import { useEffect } from "react";
import { Button } from "@/src/shared/components/ui/button";

export default function AppError({
  error,
  unstable_retry,
}: {
  error: Error & { digest?: string };
  unstable_retry: () => void;
}) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="p-6 flex flex-col items-center justify-center gap-4 py-24">
      <p className="text-muted-foreground">No se pudieron cargar los datos.</p>
      <Button variant="outline" onClick={() => unstable_retry()}>
        Reintentar
      </Button>
    </div>
  );
}
