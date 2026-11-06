"use client";

import { useState, useTransition } from "react";
import { Pencil, Plus } from "lucide-react";
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
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { SharedExpense, SharedGroupDetail } from "../types/shared";
import { saveExpenseAction } from "../actions/expenses";
import {
  SPLIT_MODE_LABELS,
  SplitMode,
  defaultDateFor,
  detectMode,
  distribute,
  toCents,
} from "../utils/split";

// Radix Select doesn't accept "" as an item value.
const NONE = "none";

interface ExpenseSheetProps {
  group: SharedGroupDetail;
  year: number;
  month: number;
  expense?: SharedExpense;
}

interface Person {
  userId: string;
  name: string;
}

interface Draft {
  date: string;
  description: string;
  total: string;
  categoryId: string;
  paidByUserId: string;
  mode: SplitMode;
  // Per person, in the same order as `people`.
  included: boolean[];
  percents: string[];
  amounts: string[];
}

// Members of the group plus anyone already on the expense who has left since.
function peopleFor(group: SharedGroupDetail, expense?: SharedExpense): Person[] {
  const people: Person[] = group.members.map((m) => ({
    userId: m.userId,
    name: m.isMe ? `${m.name} (vos)` : m.name,
  }));
  for (const s of expense?.splits ?? []) {
    if (!people.some((p) => p.userId === s.userId)) people.push({ userId: s.userId, name: s.name });
  }
  return people;
}

function draftFrom(group: SharedGroupDetail, people: Person[], year: number, month: number, expense?: SharedExpense): Draft {
  const me = group.members.find((m) => m.isMe)?.userId ?? people[0].userId;
  if (!expense) {
    const even = Math.floor(100 / people.length);
    return {
      date: defaultDateFor(year, month),
      description: "",
      total: "",
      categoryId: NONE,
      paidByUserId: me,
      mode: "equal",
      included: people.map(() => true),
      percents: people.map((_, i) => String(i === 0 ? 100 - even * (people.length - 1) : even)),
      amounts: people.map(() => ""),
    };
  }

  const totalCents = toCents(expense.total);
  const parts = people.map((p) => toCents(expense.splits.find((s) => s.userId === p.userId)?.amount ?? 0));
  const { mode, percents } = detectMode(totalCents, parts);
  return {
    date: expense.date,
    description: expense.description,
    total: String(expense.total),
    categoryId: expense.categoryId ?? NONE,
    paidByUserId: expense.paidByUserId,
    mode,
    included: parts.map((p) => p > 0),
    percents: mode === "percent" ? percents.map(String) : people.map(() => ""),
    amounts: parts.map((p) => (p > 0 ? String(p / 100) : "")),
  };
}

// Returns the parts in cents, or an error message explaining why they're invalid.
function computeParts(draft: Draft, totalCents: number): number[] | string {
  switch (draft.mode) {
    case "equal": {
      if (!draft.included.some(Boolean)) return "Elegí al menos una persona.";
      return distribute(totalCents, draft.included.map((i) => (i ? 1 : 0)));
    }
    case "percent": {
      const percents = draft.percents.map((p) => (p.trim() === "" ? 0 : Number(p.replace(",", "."))));
      if (percents.some((p) => !Number.isFinite(p) || p < 0)) return "Los porcentajes no son válidos.";
      const sum = percents.reduce((a, b) => a + b, 0);
      if (Math.abs(sum - 100) > 0.001) return `Los porcentajes suman ${sum}%, tienen que sumar 100%.`;
      return distribute(totalCents, percents);
    }
    case "amount": {
      const parts = draft.amounts.map((a) => (a.trim() === "" ? 0 : toCents(a)));
      if (parts.some((p) => Number.isNaN(p) || p < 0)) return "Los montos no son válidos.";
      const sum = parts.reduce((a, b) => a + b, 0);
      if (sum !== totalCents) {
        const diff = (totalCents - sum) / 100;
        return diff > 0
          ? `Falta asignar ${formatCurrency(diff)}.`
          : `Te pasaste por ${formatCurrency(-diff)}.`;
      }
      return parts;
    }
  }
}

export default function ExpenseSheet({ group, year, month, expense }: ExpenseSheetProps) {
  const isEdit = !!expense;
  const people = peopleFor(group, expense);
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<Draft>(() => draftFrom(group, people, year, month, expense));
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const totalCents = toCents(draft.total || "0");
  const preview = totalCents > 0 ? computeParts(draft, totalCents) : null;

  function handleOpenChange(next: boolean) {
    if (next) {
      setDraft(draftFrom(group, people, year, month, expense));
      setError(null);
    }
    setOpen(next);
  }

  function update<K extends keyof Draft>(key: K, value: Draft[K]) {
    setDraft((d) => ({ ...d, [key]: value }));
  }

  function updateAt(key: "included" | "percents" | "amounts", index: number, value: boolean | string) {
    setDraft((d) => {
      const list = [...d[key]] as (boolean | string)[];
      list[index] = value;
      return { ...d, [key]: list };
    });
  }

  function handleSave() {
    if (!draft.description.trim()) return setError("Ingresá una descripción.");
    if (!draft.date) return setError("Elegí una fecha.");
    if (!(totalCents > 0)) return setError("El total debe ser mayor a cero.");
    const parts = computeParts(draft, totalCents);
    if (typeof parts === "string") return setError(parts);

    setError(null);
    startTransition(async () => {
      const result = await saveExpenseAction(
        group.id,
        {
          date: draft.date,
          description: draft.description.trim(),
          total: totalCents / 100,
          categoryId: draft.categoryId === NONE ? null : draft.categoryId,
          paidByUserId: draft.paidByUserId,
          splits: people
            .map((p, i) => ({ userId: p.userId, amount: parts[i] / 100 }))
            .filter((s) => s.amount > 0),
        },
        expense?.id,
      );
      if (result.status === "error") {
        setError(result.message);
        return;
      }
      setOpen(false);
    });
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        {isEdit ? (
          <Button variant="ghost" size="icon-sm" aria-label="Editar">
            <Pencil className="h-4 w-4" />
          </Button>
        ) : (
          <Button>
            <Plus className="h-4 w-4" />
            Nuevo gasto
          </Button>
        )}
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>{isEdit ? "Editar gasto" : "Nuevo gasto en común"}</SheetTitle>
          <SheetDescription>
            Todos los miembros de {group.name} ven y pueden editar este gasto.
          </SheetDescription>
        </SheetHeader>

        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel htmlFor="shared-expense-description">Descripción</FieldLabel>
            <Input
              id="shared-expense-description"
              value={draft.description}
              onChange={(e) => update("description", e.target.value)}
              placeholder="Ej: Supermercado"
              maxLength={200}
            />
          </Field>

          <div className="grid grid-cols-2 gap-3">
            <Field>
              <FieldLabel htmlFor="shared-expense-date">Fecha</FieldLabel>
              <Input
                id="shared-expense-date"
                type="date"
                value={draft.date}
                onChange={(e) => update("date", e.target.value)}
              />
            </Field>
            <Field>
              <FieldLabel htmlFor="shared-expense-total">Total</FieldLabel>
              <Input
                id="shared-expense-total"
                type="number"
                step="0.01"
                min="0.01"
                placeholder="0.00"
                value={draft.total}
                onChange={(e) => update("total", e.target.value)}
              />
            </Field>
          </div>

          <Field>
            <FieldLabel>Categoría</FieldLabel>
            <Select value={draft.categoryId} onValueChange={(v) => update("categoryId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={NONE}>Sin categoría</SelectItem>
                {group.categories.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <Field>
            <FieldLabel>Pagó</FieldLabel>
            <Select value={draft.paidByUserId} onValueChange={(v) => update("paidByUserId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {people.map((p) => (
                  <SelectItem key={p.userId} value={p.userId}>
                    {p.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <div className="flex flex-col gap-3 rounded-lg border p-3">
            <Field>
              <FieldLabel>División</FieldLabel>
              <Select value={draft.mode} onValueChange={(v) => update("mode", v as SplitMode)}>
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {(Object.keys(SPLIT_MODE_LABELS) as SplitMode[]).map((m) => (
                    <SelectItem key={m} value={m}>
                      {SPLIT_MODE_LABELS[m]}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>

            <div className="flex flex-col gap-2">
              {people.map((p, i) => {
                const part = Array.isArray(preview) ? preview[i] / 100 : null;
                return (
                  <div key={p.userId} className="flex items-center gap-2 text-sm">
                    {draft.mode === "equal" ? (
                      <label className="flex flex-1 items-center gap-2">
                        <input
                          type="checkbox"
                          className="size-4 accent-primary"
                          checked={draft.included[i]}
                          onChange={(e) => updateAt("included", i, e.target.checked)}
                        />
                        {p.name}
                      </label>
                    ) : (
                      <span className="flex-1">{p.name}</span>
                    )}

                    {draft.mode === "percent" && (
                      <div className="flex items-center gap-1">
                        <Input
                          aria-label={`Porcentaje de ${p.name}`}
                          type="number"
                          min="0"
                          max="100"
                          step="1"
                          className="h-8 w-20 text-right"
                          value={draft.percents[i]}
                          onChange={(e) => updateAt("percents", i, e.target.value)}
                        />
                        <span className="text-muted-foreground">%</span>
                      </div>
                    )}

                    {draft.mode === "amount" ? (
                      <Input
                        aria-label={`Monto de ${p.name}`}
                        type="number"
                        min="0"
                        step="0.01"
                        placeholder="0.00"
                        className="h-8 w-32 text-right"
                        value={draft.amounts[i]}
                        onChange={(e) => updateAt("amounts", i, e.target.value)}
                      />
                    ) : (
                      <span className="w-28 text-right tabular-nums text-muted-foreground">
                        {part !== null ? formatCurrency(part) : "—"}
                      </span>
                    )}
                  </div>
                );
              })}
            </div>
            {typeof preview === "string" && (
              <p className="text-xs text-muted-foreground">{preview}</p>
            )}
          </div>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <Separator />

          <Button onClick={handleSave} disabled={isPending}>
            {isPending ? "Guardando..." : isEdit ? "Guardar cambios" : "Agregar"}
          </Button>
        </div>
      </SheetContent>
    </Sheet>
  );
}
