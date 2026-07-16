"use client";

import { useState, useTransition } from "react";
import { replicatePreviousBudgetAction } from "../actions/replicate-previous-budget";
import { Button } from "@/src/shared/components/ui/button";
import { Copy } from "lucide-react";
import AlertConfirm from "@/src/shared/components/alert-confirm";

interface ReplicateBudgetButtonProps {
  year: number;
  month: number;
}

export default function ReplicateBudgetButton({ year, month }: ReplicateBudgetButtonProps) {
  const [isPending, startTransition] = useTransition();
  const [alertConfirmOpen, setAlertConfirmOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const onAlertConfirm = () => {
    setAlertConfirmOpen(false);
    setError(null);
    startTransition(async () => {
      const result = await replicatePreviousBudgetAction(year, month);
      if (result.status === "error") {
        setError(result.message);
      }
    });
  };

  return (
    <>
      <AlertConfirm
        title="¿Replicar presupuesto del mes anterior?"
        description="Se van a copiar los montos presupuestados del mes anterior a este período."
        confirmText="Replicar"
        open={alertConfirmOpen}
        onConfirm={onAlertConfirm}
        onCancel={() => setAlertConfirmOpen(false)}
      />
      <div className="flex flex-col items-center gap-1">
        <Button
          variant="outline"
          size="sm"
          disabled={isPending}
          onClick={() => setAlertConfirmOpen(true)}
        >
          <Copy className="h-4 w-4 mr-2" />
          Replicar mes anterior
        </Button>
        {error && <p className="text-xs text-destructive">{error}</p>}
      </div>
    </>
  );
}
