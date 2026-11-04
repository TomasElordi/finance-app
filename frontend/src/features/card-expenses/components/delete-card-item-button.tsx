"use client";

import { useState, useTransition } from "react";
import { Trash2 } from "lucide-react";
import { Button } from "@/src/shared/components/ui/button";
import AlertConfirm from "@/src/shared/components/alert-confirm";
import { CardItem } from "../types/card";
import { deleteCardItemAction } from "../actions/delete-card-item";

export default function DeleteCardItemButton({ item }: { item: CardItem }) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleConfirm() {
    setConfirmOpen(false);
    startTransition(async () => {
      const result = await deleteCardItemAction(item.id);
      setError(result.status === "error" ? result.message : null);
    });
  }

  return (
    <>
      <AlertConfirm
        title={`¿Eliminar "${item.name}"?`}
        description={
          item.installments > 1
            ? `Se eliminan las ${item.installments} cuotas de esta compra.`
            : "Esta acción no se puede deshacer."
        }
        open={confirmOpen}
        onConfirm={handleConfirm}
        onCancel={() => setConfirmOpen(false)}
      />
      <Button
        variant="ghost"
        size="icon-sm"
        aria-label="Eliminar"
        disabled={isPending}
        onClick={() => setConfirmOpen(true)}
        title={error ?? undefined}
      >
        <Trash2 className="h-4 w-4 text-destructive" />
      </Button>
    </>
  );
}
