"use client";

import { useActionState, useState, useTransition } from "react";
import { closeMonthAction } from "@/src/features/entries/actions/close-month";
import { Button } from "@/src/shared/components/ui/button";
import { CalendarCheck } from "lucide-react";
import { ActionState } from "@/src/shared/types/action-state";
import AlertConfirm from "@/src/shared/components/alert-confirm";

const initialState: ActionState = { status: "idle" };

export default function CloseMonthButton() {
  const [state, formAction, pending] = useActionState(
    closeMonthAction,
    initialState,
  );
  const [isPending, startTransition] = useTransition();
  const [alertConfirmOpen, setAlertConfirmOpen] = useState(false);

  const handleClose = () => {
    setAlertConfirmOpen(true);
  };
  const onAlertConfirm = () => {
    startTransition(() => formAction(new FormData()));
    setAlertConfirmOpen(false);
  };
  const onAlertCancel = () => {
    setAlertConfirmOpen(false);
  };

  return (
    <>
      <AlertConfirm
        title="¿Cerrar el mes?"
        description="Se van a cancelar los saldos de todas las cuentas de Resultado Positivo y Negativo, transfiriendo la diferencia a Resultados Anteriores. Esta acción no se puede deshacer."
        confirmText="Cerrar mes"
        open={alertConfirmOpen}
        onConfirm={onAlertConfirm}
        onCancel={onAlertCancel}
      />
      <form action={handleClose}>
        <Button
          type="submit"
          variant="outline"
          disabled={pending || isPending}
        >
          <CalendarCheck className="h-4 w-4" />
          Cerrar mes
        </Button>
        {state.status === "error" && (
          <p className="text-xs text-destructive mt-1">{state.message}</p>
        )}
      </form>
    </>
  );
}
