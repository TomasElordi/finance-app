"use client";

import { useActionState, useState, useTransition } from "react";
import { deleteAccountAction } from "@/src/features/accounts/actions/delete-account";
import { Button } from "@/src/shared/components/ui/button";
import { Trash2 } from "lucide-react";
import { ActionState } from "@/src/shared/types/action-state";
import AlertConfirm from "@/src/shared/components/alert-confirm";

const initialState: ActionState = { status: "idle" };

export default function DeleteAccountForm({ id }: { id: string }) {
  const [state, formAction, pending] = useActionState(
    deleteAccountAction,
    initialState,
  );

  const [isPending, startTransition] = useTransition();
  const [alertConfirmOpen, setAlertConfirmOpen] = useState(false);
  const [payload, setPayload] = useState<FormData>();
  const handleDelete = (payload: FormData) => {
    setPayload(payload);
    setAlertConfirmOpen(true);
  };
  const onAlertConfirm = () => {
    if (!payload) return;
    startTransition(() => formAction(payload));
    setAlertConfirmOpen(false);
  };
  const onAlertCancel = () => {
    setAlertConfirmOpen(false);
  };

  return (
    <>
      <AlertConfirm
        title="¿Estás seguro que quieres eliminar esta cuenta?"
        description="Esta acción no se puede deshacer."
        open={alertConfirmOpen}
        onConfirm={onAlertConfirm}
        onCancel={onAlertCancel}
      />
      <form action={handleDelete}>
        <input type="hidden" name="id" value={id} />
        <Button
          type="submit"
          variant="ghost"
          size="icon-sm"
          disabled={pending || isPending}
          aria-label="Eliminar cuenta"
        >
          <Trash2 className="h-4 w-4 text-destructive" />
        </Button>
        {state.status === "error" && (
          <p className="text-xs text-destructive mt-1">{state.message}</p>
        )}
      </form>
    </>
  );
}
