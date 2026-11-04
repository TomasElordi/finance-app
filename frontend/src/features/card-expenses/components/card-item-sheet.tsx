"use client";

import { useState, useTransition } from "react";
import { Pencil, Plus } from "lucide-react";
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
import { Field, FieldLabel } from "@/src/shared/components/ui/field";
import { Separator } from "@/src/shared/components/ui/separator";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import {
  CARD_ITEM_TYPE_LABELS,
  CardItem,
  CardItemType,
  Person,
  startedBefore,
} from "../types/card";
import { saveCardItemAction } from "../actions/save-card-item";
import { createPersonAction } from "../actions/create-person";

// Radix Select doesn't accept "" as an item value.
const NONE = "none";
const NEW_PERSON = "new";

type Payment = "single" | "installments" | "recurring";

const PAYMENT_LABELS: Record<Payment, string> = {
  single: "Un pago",
  installments: "En cuotas",
  recurring: "Recurrente (todos los meses)",
};

interface CardItemSheetProps {
  accounts: Account[];
  people: Person[];
  year: number;
  month: number;
  item?: CardItem;
}

interface Draft {
  name: string;
  amount: string;
  type: CardItemType;
  accountId: string;
  personId: string;
  payment: Payment;
  currentInstallment: string;
  installments: string;
  fromThisMonth: boolean;
}

function draftFrom(item?: CardItem): Draft {
  return {
    name: item?.name ?? "",
    amount: item ? String(item.amount) : "",
    type: item?.type ?? "Expense",
    accountId: item?.accountId ?? NONE,
    personId: item?.personId ?? NONE,
    payment: item?.isRecurring
      ? "recurring"
      : (item?.installments ?? 1) > 1
        ? "installments"
        : "single",
    currentInstallment: String(item && !item.isRecurring ? item.installmentNumber : 1),
    installments: String(item && item.installments > 1 ? item.installments : 2),
    fromThisMonth: true,
  };
}

export default function CardItemSheet({ accounts, people, year, month, item }: CardItemSheetProps) {
  const isEdit = !!item;
  // Only a recurring item already charged in earlier months can be edited "from this month on".
  const canSplit = !!item && startedBefore(item, year, month);
  const [open, setOpen] = useState(false);
  const [draft, setDraft] = useState<Draft>(() => draftFrom(item));
  const [extraPeople, setExtraPeople] = useState<Person[]>([]);
  const [newPersonName, setNewPersonName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const allPeople = [...people, ...extraPeople.filter((p) => !people.some((q) => q.id === p.id))];

  function handleOpenChange(next: boolean) {
    if (next) {
      setDraft(draftFrom(item));
      setNewPersonName("");
      setError(null);
    }
    setOpen(next);
  }

  function update<K extends keyof Draft>(key: K, value: Draft[K]) {
    setDraft((d) => ({ ...d, [key]: value }));
  }

  function handleCreatePerson() {
    const name = newPersonName.trim();
    if (!name) return;
    setError(null);
    startTransition(async () => {
      const result = await createPersonAction(name);
      if (result.status === "error") {
        setError(result.message);
        return;
      }
      setExtraPeople((p) => [...p, result.person]);
      update("personId", result.person.id);
      setNewPersonName("");
    });
  }

  function handleSave() {
    const amount = Number(draft.amount);
    const payment: Payment = draft.type === "Expense" ? draft.payment : "single";
    const usesInstallments = payment === "installments";
    const installments = usesInstallments ? Number(draft.installments) : 1;
    const currentInstallment = usesInstallments ? Number(draft.currentInstallment) : 1;

    if (!draft.name.trim()) return setError("Ingresá un nombre.");
    if (!(amount > 0)) return setError("El monto debe ser mayor a cero.");
    if (draft.personId === NEW_PERSON) return setError("Creá la persona o elegí otra.");
    if (usesInstallments) {
      if (!Number.isInteger(installments) || installments < 2 || installments > 60)
        return setError("El total de cuotas debe estar entre 2 y 60.");
      if (!Number.isInteger(currentInstallment) || currentInstallment < 1 || currentInstallment > installments)
        return setError("La cuota actual debe estar entre 1 y el total.");
    }

    setError(null);
    startTransition(async () => {
      const result = await saveCardItemAction(
        {
          name: draft.name.trim(),
          amount,
          type: draft.type,
          accountId: draft.accountId === NONE ? null : draft.accountId,
          personId: draft.personId === NONE ? null : draft.personId,
          year,
          month,
          currentInstallment,
          installments,
          isRecurring: payment === "recurring",
          fromThisMonth: canSplit && draft.fromThisMonth,
        },
        item?.id,
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
            Nuevo ítem
          </Button>
        )}
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>{isEdit ? "Editar ítem" : "Nuevo ítem"}</SheetTitle>
          <SheetDescription>
            {isEdit && item.installments > 1
              ? "Los cambios aplican a todas las cuotas de esta compra."
              : isEdit && item.isRecurring
                ? "Gasto recurrente: se repite todos los meses."
                : "Un gasto, impuesto o saldo a favor del resumen de la tarjeta."}
          </SheetDescription>
        </SheetHeader>

        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel>Tipo</FieldLabel>
            <Select value={draft.type} onValueChange={(v) => update("type", v as CardItemType)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {(Object.keys(CARD_ITEM_TYPE_LABELS) as CardItemType[]).map((t) => (
                  <SelectItem key={t} value={t}>
                    {CARD_ITEM_TYPE_LABELS[t]}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          <Field>
            <FieldLabel htmlFor="card-item-name">Nombre</FieldLabel>
            <Input
              id="card-item-name"
              value={draft.name}
              onChange={(e) => update("name", e.target.value)}
              placeholder={draft.type === "Tax" ? "Ej: IVA" : "Ej: Supermercado"}
              maxLength={200}
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="card-item-amount">
              {draft.type === "Expense" && draft.payment === "installments"
                ? "Monto de la cuota"
                : draft.type === "Expense" && draft.payment === "recurring"
                  ? "Monto mensual"
                  : "Monto"}
            </FieldLabel>
            <Input
              id="card-item-amount"
              type="number"
              step="0.01"
              min="0.01"
              placeholder="0.00"
              value={draft.amount}
              onChange={(e) => update("amount", e.target.value)}
            />
          </Field>

          {draft.type === "Expense" && (
            <div className="flex flex-col gap-3 rounded-lg border p-3">
              <Field>
                <FieldLabel>Pago</FieldLabel>
                <Select value={draft.payment} onValueChange={(v) => update("payment", v as Payment)}>
                  <SelectTrigger className="w-full">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {(Object.keys(PAYMENT_LABELS) as Payment[]).map((p) => (
                      <SelectItem key={p} value={p}>
                        {PAYMENT_LABELS[p]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
              {draft.payment === "recurring" && !isEdit && (
                <p className="text-xs text-muted-foreground">
                  Aparece en este resumen y en todos los siguientes hasta que lo des de baja.
                </p>
              )}
              {draft.payment === "installments" && (
                <div className="flex items-end gap-2">
                  <Field>
                    <FieldLabel htmlFor="card-item-current">Cuota actual</FieldLabel>
                    <Input
                      id="card-item-current"
                      type="number"
                      min="1"
                      value={draft.currentInstallment}
                      onChange={(e) => update("currentInstallment", e.target.value)}
                    />
                  </Field>
                  <span className="pb-2 text-muted-foreground">de</span>
                  <Field>
                    <FieldLabel htmlFor="card-item-total">Total</FieldLabel>
                    <Input
                      id="card-item-total"
                      type="number"
                      min="2"
                      max="60"
                      value={draft.installments}
                      onChange={(e) => update("installments", e.target.value)}
                    />
                  </Field>
                </div>
              )}
            </div>
          )}

          <Field>
            <FieldLabel>A quién corresponde</FieldLabel>
            <Select value={draft.personId} onValueChange={(v) => update("personId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={NONE}>Yo</SelectItem>
                {allPeople.map((p) => (
                  <SelectItem key={p.id} value={p.id}>
                    {p.name}
                  </SelectItem>
                ))}
                <SelectItem value={NEW_PERSON}>+ Nueva persona…</SelectItem>
              </SelectContent>
            </Select>
            {draft.personId === NEW_PERSON && (
              <div className="flex gap-2">
                <Input
                  autoFocus
                  placeholder="Nombre"
                  value={newPersonName}
                  maxLength={100}
                  onChange={(e) => setNewPersonName(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      handleCreatePerson();
                    }
                  }}
                />
                <Button type="button" variant="outline" onClick={handleCreatePerson} disabled={isPending}>
                  Agregar
                </Button>
              </div>
            )}
          </Field>

          <Field>
            <FieldLabel>Cuenta asociada (opcional)</FieldLabel>
            <Select value={draft.accountId} onValueChange={(v) => update("accountId", v)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value={NONE}>Sin cuenta</SelectItem>
                {accounts.map((a) => (
                  <SelectItem key={a.id} value={a.id}>
                    {a.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Field>

          {canSplit && (
            <Field>
              <FieldLabel>Aplicar cambios</FieldLabel>
              <Select
                value={draft.fromThisMonth ? "from" : "all"}
                onValueChange={(v) => update("fromThisMonth", v === "from")}
              >
                <SelectTrigger className="w-full">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="from">Desde este mes en adelante</SelectItem>
                  <SelectItem value="all">En todos los meses</SelectItem>
                </SelectContent>
              </Select>
            </Field>
          )}

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
