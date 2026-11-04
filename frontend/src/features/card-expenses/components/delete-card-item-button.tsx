"use client";

import { useState, useTransition } from "react";
import { Trash2 } from "lucide-react";
import { Button } from "@/src/shared/components/ui/button";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/src/shared/components/ui/alert-dialog";
import { CardItem, startedBefore } from "../types/card";
import { deleteCardItemAction } from "../actions/delete-card-item";

interface DeleteCardItemButtonProps {
  item: CardItem;
  year: number;
  month: number;
}

export default function DeleteCardItemButton({ item, year, month }: DeleteCardItemButtonProps) {
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();
  const canStopFromHere = startedBefore(item, year, month);

  function run(from?: { year: number; month: number }) {
    setConfirmOpen(false);
    startTransition(async () => {
      const result = await deleteCardItemAction(item.id, from);
      setError(result.status === "error" ? result.message : null);
    });
  }

  const description = canStopFromHere
    ? "Podés darlo de baja desde este mes (los meses anteriores no cambian) o eliminarlo de todos los meses."
    : item.installments > 1
      ? `Se eliminan las ${item.installments} cuotas de esta compra.`
      : item.isRecurring
        ? "Se elimina de este mes y de todos los siguientes."
        : "Esta acción no se puede deshacer.";

  return (
    <>
      <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{`¿Eliminar "${item.name}"?`}</AlertDialogTitle>
            <AlertDialogDescription>{description}</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            {canStopFromHere && (
              <Button variant="outline" onClick={() => run()}>
                Eliminar de todos los meses
              </Button>
            )}
            <Button
              variant={canStopFromHere ? "default" : "destructive"}
              onClick={() => run(canStopFromHere ? { year, month } : undefined)}
            >
              {canStopFromHere ? "Dar de baja desde este mes" : "Eliminar"}
            </Button>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
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
