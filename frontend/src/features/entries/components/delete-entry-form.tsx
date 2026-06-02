"use client";

import { useActionState } from "react";
import { deleteEntryAction } from "@/src/features/entries/actions/delete-entry";
import { Button } from "@/src/shared/components/ui/button";
import { Trash2 } from "lucide-react";
import { ActionState } from "@/src/shared/types/action-state";

const initialState: ActionState = { status: "idle" };

export default function DeleteEntryForm({ id }: { id: string }) {
  const [state, formAction, pending] = useActionState(
    deleteEntryAction,
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
        aria-label="Eliminar asiento"
      >
        <Trash2 className="h-4 w-4 text-destructive" />
      </Button>
      {state.status === "error" && (
        <p className="text-xs text-destructive mt-1">{state.message}</p>
      )}
    </form>
  );
}
