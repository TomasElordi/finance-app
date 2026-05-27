"use client";

import { useActionState, useEffect, useState } from "react";
import { createAccountAction } from "@/src/features/accounts/actions/create-account";
import { CreateAccountActionState } from "@/src/features/accounts/types/create-account-action-state";
import { NatureType } from "@/src/features/accounts/types/account";
import {
  Sheet,
  SheetContent,
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

const initialState: CreateAccountActionState = { status: "idle" };

const NATURE_OPTIONS = [
  { value: NatureType.Asset, label: "Asset" },
  { value: NatureType.Liability, label: "Liability" },
  { value: NatureType.Equity, label: "Equity" },
  { value: NatureType.Income, label: "Income" },
  { value: NatureType.Expense, label: "Expense" },
];

export default function CreateAccountSheet() {
  const [open, setOpen] = useState(false);
  const [state, formAction, pending] = useActionState(
    createAccountAction,
    initialState,
  );

  useEffect(() => {
    if (state.status === "success") {
      setOpen(false);
    }
  }, [state.status]);

  const nameError = state.status === "error" && !!state.errors.name;
  const natureError = state.status === "error" && !!state.errors.nature;

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button>Nueva cuenta</Button>
      </SheetTrigger>
      <SheetContent>
        <SheetHeader>
          <SheetTitle>Nueva cuenta</SheetTitle>
        </SheetHeader>
        <form action={formAction} className="flex flex-col gap-4 p-4">
          <Field data-invalid={nameError}>
            <FieldLabel htmlFor="name">Nombre</FieldLabel>
            <Input
              id="name"
              name="name"
              type="text"
              placeholder="Ej: Caja de ahorro"
              aria-invalid={nameError}
              required
            />
            {nameError && (
              <FieldDescription className="text-destructive">
                {state.errors.name}
              </FieldDescription>
            )}
          </Field>

          <Field data-invalid={natureError}>
            <FieldLabel htmlFor="nature">Tipo</FieldLabel>
            <select
              id="nature"
              name="nature"
              defaultValue={NatureType.Asset}
              aria-invalid={natureError}
              className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            >
              {NATURE_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
            {natureError && (
              <FieldDescription className="text-destructive">
                {state.errors.nature}
              </FieldDescription>
            )}
          </Field>

          {state.status === "error" && state.message && (
            <p className="text-sm text-destructive">{state.message}</p>
          )}

          <Separator />

          <Button type="submit" disabled={pending}>
            {pending ? "Creando..." : "Crear cuenta"}
          </Button>
        </form>
      </SheetContent>
    </Sheet>
  );
}
