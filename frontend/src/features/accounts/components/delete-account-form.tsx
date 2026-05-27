"use client";

import { useActionState } from "react";
import { deleteAccountAction } from "@/src/features/accounts/actions/delete-account";
import { Button } from "@/src/shared/components/ui/button";
import { Trash2 } from "lucide-react";
import { ActionState } from "@/src/shared/types/action-state";

const initialState: ActionState = { status: "idle" };

export default function DeleteAccountForm({ id }: { id: string }) {
  const [state, formAction, pending] = useActionState(
    deleteAccountAction,
    initialState,
  );

  return (
    <form action={formAction}>
      <input type="hidden" name="id" value={id} />
      <Button
        type="submit"
        variant="ghost"
        size="icon-sm"
        disabled={pending}
        aria-label="Eliminar cuenta"
      >
        <Trash2 className="h-4 w-4 text-destructive" />
      </Button>
      {state.status === "error" && (
        <p className="text-xs text-destructive mt-1">{state.message}</p>
      )}
    </form>
  );
}
