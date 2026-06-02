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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";

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
            <FieldLabel>Tipo</FieldLabel>
            <Select
              name="nature"
              defaultValue={String(NatureType.Asset)}
            >
              <SelectTrigger className="w-full" aria-invalid={natureError}>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {NATURE_OPTIONS.map((opt) => (
                  <SelectItem key={opt.value} value={String(opt.value)}>
                    {opt.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
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
