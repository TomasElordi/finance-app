"use client";

import { useActionState, useEffect, useRef, useState } from "react";
import { createEntryAction } from "@/src/features/entries/actions/create-entry";
import { CreateEntryActionState } from "@/src/features/entries/types/create-entry-action-state";
import { Account } from "@/src/features/accounts/types/account";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/src/shared/components/ui/sheet";
import { Button } from "@/src/shared/components/ui/button";
import { Input } from "@/src/shared/components/ui/input";
import {
  Field,
  FieldDescription,
  FieldLabel,
} from "@/src/shared/components/ui/field";
import { Separator } from "@/src/shared/components/ui/separator";
import EntryLinesFields from "./entry-lines-fields";

const initialState: CreateEntryActionState = { status: "idle" };

interface CreateEntrySheetProps {
  accounts: Account[];
}

export default function CreateEntrySheet({ accounts }: CreateEntrySheetProps) {
  const [open, setOpen] = useState(false);
  const [resetKey, setResetKey] = useState(0);
  const [timezoneOffset, setTimezoneOffset] = useState(0);
  useEffect(() => {
    setTimezoneOffset(new Date().getTimezoneOffset());
  }, []);

  const [state, formAction, pending] = useActionState(
    createEntryAction,
    initialState,
  );

  useEffect(() => {
    if (state.status === "success") {
      setOpen(false);
      setResetKey((k) => k + 1);
    }
  }, [state.status]);

  useEffect(() => {
    setTimezoneOffset(new Date().getTimezoneOffset());
  }, []);
  const titleError = state.status === "error" && !!state.errors.title;
  const dateError = state.status === "error" && !!state.errors.date;
  const entryLinesError = state.status === "error" && !!state.errors.entryLines;

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button>Nuevo asiento</Button>
      </SheetTrigger>
      <SheetContent className="sm:max-w-xl overflow-y-auto md:min-w-125">
        <SheetHeader>
          <SheetTitle>Nuevo asiento</SheetTitle>
          <SheetDescription>
            Registra un nuevo movimiento contable con sus cuentas del debe y
            haber.
          </SheetDescription>
        </SheetHeader>
        <form action={formAction} className="flex flex-col gap-4 p-4">
          <input
            type="hidden"
            name="timezoneOffset"
            value={timezoneOffset}
          ></input>
          <Field data-invalid={titleError}>
            <FieldLabel htmlFor="title">Título</FieldLabel>
            <Input
              id="title"
              name="title"
              type="text"
              placeholder="Ej: Pago de alquiler"
              aria-invalid={titleError}
              required
            />
            {titleError && (
              <FieldDescription className="text-destructive">
                {state.errors.title}
              </FieldDescription>
            )}
          </Field>

          <Field>
            <FieldLabel htmlFor="description">
              Descripción (opcional)
            </FieldLabel>
            <Input
              id="description"
              name="description"
              type="text"
              placeholder="Notas adicionales"
            />
          </Field>

          <Field data-invalid={dateError}>
            <FieldLabel htmlFor="date">Fecha</FieldLabel>
            <Input
              id="date"
              name="date"
              type="date"
              aria-invalid={dateError}
              required
            />
            {dateError && (
              <FieldDescription className="text-destructive">
                {state.errors.date}
              </FieldDescription>
            )}
          </Field>

          <div className="flex flex-col gap-2">
            <FieldLabel>Líneas del asiento</FieldLabel>
            <EntryLinesFields key={resetKey} accounts={accounts} />
            {entryLinesError && (
              <p className="text-sm text-destructive">
                {state.errors.entryLines}
              </p>
            )}
          </div>

          {state.status === "error" && state.message && (
            <p className="text-sm text-destructive">{state.message}</p>
          )}

          <Separator />

          <Button type="submit" disabled={pending}>
            {pending ? "Creando..." : "Crear asiento"}
          </Button>
        </form>
      </SheetContent>
    </Sheet>
  );
}
