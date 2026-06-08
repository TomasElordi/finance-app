"use client";

import { useActionState, useEffect, useState } from "react";
import { updateEntryAction } from "@/src/features/entries/actions/update-entry";
import { UpdateEntryActionState } from "@/src/features/entries/types/update-entry-action-state";
import { Entry } from "@/src/features/entries/types/entry";
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
import { Pencil } from "lucide-react";
import EntryLinesFields from "./entry-lines-fields";

const initialState: UpdateEntryActionState = { status: "idle" };

interface EditEntrySheetProps {
  entry: Entry;
  accounts: Account[];
}

export default function EditEntrySheet({
  entry,
  accounts,
}: EditEntrySheetProps) {
  const [open, setOpen] = useState(false);
  const [resetKey, setResetKey] = useState(0);
  const [state, formAction, pending] = useActionState(
    updateEntryAction,
    initialState,
  );

  useEffect(() => {
    if (state.status === "success") {
      setOpen(false);
      setResetKey((k) => k + 1);
    }
  }, [state.status]);

  const titleError = state.status === "error" && !!state.errors.title;
  const dateError = state.status === "error" && !!state.errors.date;
  const entryLinesError = state.status === "error" && !!state.errors.entryLines;

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon-sm" aria-label="Editar asiento">
          <Pencil className="h-4 w-4" />
        </Button>
      </SheetTrigger>
      <SheetContent className="sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Editar asiento</SheetTitle>
          <SheetDescription>
            Modifica los datos del asiento contable.
          </SheetDescription>
        </SheetHeader>
        <form action={formAction} className="flex flex-col gap-4 p-4">
          <input type="hidden" name="id" value={entry.id} />

          <Field data-invalid={titleError}>
            <FieldLabel htmlFor={`title-${entry.id}`}>Título</FieldLabel>
            <Input
              id={`title-${entry.id}`}
              name="title"
              type="text"
              defaultValue={entry.title}
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
            <FieldLabel htmlFor={`description-${entry.id}`}>
              Descripción (opcional)
            </FieldLabel>
            <Input
              id={`description-${entry.id}`}
              name="description"
              type="text"
              defaultValue={entry.description ?? ""}
            />
          </Field>

          <Field data-invalid={dateError}>
            <FieldLabel htmlFor={`date-${entry.id}`}>Fecha</FieldLabel>
            <Input
              id={`date-${entry.id}`}
              name="date"
              type="date"
              defaultValue={entry.date.slice(0, 10)}
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
            <EntryLinesFields
              key={resetKey}
              accounts={accounts}
              initialLines={entry.entryLines}
            />
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
            {pending ? "Guardando..." : "Guardar cambios"}
          </Button>
        </form>
      </SheetContent>
    </Sheet>
  );
}
