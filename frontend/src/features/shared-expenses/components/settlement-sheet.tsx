"use client";

import { useState, useTransition } from "react";
import { HandCoins } from "lucide-react";
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
import { Field, FieldLabel } from "@/src/shared/components/ui/field";
import { Separator } from "@/src/shared/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import { SharedDebt, SharedGroupDetail } from "../types/shared";
import { createSettlementAction } from "../actions/settlements";
import { defaultDateFor, toCents } from "../utils/split";

interface SettlementSheetProps {
  group: SharedGroupDetail;
  year: number;
  month: number;
  // When given, the form is prefilled to pay off this debt.
  debt?: SharedDebt;
}

interface Draft {
  date: string;
  fromUserId: string;
  toUserId: string;
  amount: string;
  note: string;
}

export default function SettlementSheet({ group, year, month, debt }: SettlementSheetProps) {
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<Draft>(() => initialDraft());
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  if (group.members.length < 2) return null;

  function initialDraft(): Draft {
    const me = group.members.find((m) => m.isMe)?.userId ?? group.members[0].userId;
    const other = group.members.find((m) => m.userId !== me)?.userId ?? me;
    return {
      date: defaultDateFor(year, month),
      fromUserId: debt?.fromUserId ?? me,
      toUserId: debt?.toUserId ?? other,
      amount: debt ? String(debt.amount) : "",
      note: "",
    };
  }

  function handleOpenChange(next: boolean) {
    if (next) {
      setDraft(initialDraft());
      setError(null);
    }
    setOpen(next);
  }

  function update<K extends keyof Draft>(key: K, value: Draft[K]) {
    setDraft((d) => ({ ...d, [key]: value }));
  }

  function handleSave() {
    const cents = toCents(draft.amount || "0");
    if (!(cents > 0)) return setError("El monto debe ser mayor a cero.");
    if (draft.fromUserId === draft.toUserId) return setError("Elegí dos personas distintas.");
    if (!draft.date) return setError("Elegí una fecha.");

    setError(null);
    startTransition(async () => {
      const result = await createSettlementAction(group.id, {
        date: draft.date,
        fromUserId: draft.fromUserId,
        toUserId: draft.toUserId,
        amount: cents / 100,
        note: draft.note.trim() || null,
      });
      if (result.status === "error") {
        setError(result.message);
        return;
      }
      setOpen(false);
    });
  }

  const memberLabel = (userId: string) => {
    const m = group.members.find((x) => x.userId === userId)!;
    return m.isMe ? `${m.name} (vos)` : m.name;
  };

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        <Button variant={debt ? "default" : "outline"} size="sm">
          <HandCoins className="h-4 w-4" />
          {debt ? "Saldar" : "Registrar pago"}
        </Button>
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>Registrar pago</SheetTitle>
          <SheetDescription>
            Un pago entre miembros para saldar deudas (por ejemplo, una transferencia).
          </SheetDescription>
        </SheetHeader>

        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel>Quién paga</FieldLabel>
            <Select value={draft.fromUserId} onValueChange={(v) => update("fromUserId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {group.members.map((m) => (
                  <SelectItem key={m.userId} value={m.userId}>
                    {memberLabel(m.userId)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <Field>
            <FieldLabel>A quién</FieldLabel>
            <Select value={draft.toUserId} onValueChange={(v) => update("toUserId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {group.members.map((m) => (
                  <SelectItem key={m.userId} value={m.userId}>
                    {memberLabel(m.userId)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <div className="grid grid-cols-2 gap-3">
            <Field>
              <FieldLabel htmlFor="settlement-amount">Monto</FieldLabel>
              <Input
                id="settlement-amount"
                type="number"
                step="0.01"
                min="0.01"
                placeholder="0.00"
                value={draft.amount}
                onChange={(e) => update("amount", e.target.value)}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="settlement-date">Fecha</FieldLabel>
              <Input
                id="settlement-date"
                type="date"
                value={draft.date}
                onChange={(e) => update("date", e.target.value)}
              />
            </Field>
          </div>

          <Field>
            <FieldLabel htmlFor="settlement-note">Nota (opcional)</FieldLabel>
            <Input
              id="settlement-note"
              placeholder="Ej: Transferencia"
              maxLength={200}
              value={draft.note}
              onChange={(e) => update("note", e.target.value)}
            />
          </Field>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <Separator />

          <Button onClick={handleSave} disabled={isPending}>
            {isPending ? "Guardando..." : "Registrar pago"}
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
