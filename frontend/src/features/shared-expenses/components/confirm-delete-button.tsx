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
import { ActionResult } from "../types/shared";

interface ConfirmDeleteButtonProps {
  title: string;
  description: string;
  onConfirm: () => Promise<ActionResult<unknown>>;
}

export default function ConfirmDeleteButton({ title, description, onConfirm }: ConfirmDeleteButtonProps) {
  const [open, setOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function run() {
    setOpen(false);
    startTransition(async () => {
      const result = await onConfirm();
      setError(result.status === "error" ? result.message : null);
    });
  }

  return (
    <>
      <AlertDialog open={open} onOpenChange={setOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{title}</AlertDialogTitle>
            <AlertDialogDescription>{description}</AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <Button variant="destructive" onClick={run}>
              Eliminar
            </Button>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <Button
        variant="ghost"
        size="icon-sm"
        aria-label="Eliminar"
        disabled={isPending}
        onClick={() => setOpen(true)}
        title={error ?? undefined}
      >
        <Trash2 className="h-4 w-4 text-destructive" />
      </Button>
    </>
  );
}
