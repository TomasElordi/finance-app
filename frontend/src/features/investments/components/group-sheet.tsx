"use client";

import { useState, useTransition } from "react";
import { Pencil, Plus, Trash2 } from "lucide-react";
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
import { GroupColor, InvestmentGroup } from "../types/investment";
import { GROUP_COLORS } from "../utils/format";
import { deleteGroupAction, saveGroupAction } from "../actions/investment-actions";

interface GroupSheetProps {
  group?: InvestmentGroup;
}

export default function GroupSheet({ group }: GroupSheetProps) {
  const isEdit = !!group;
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [color, setColor] = useState<GroupColor>("blue");
  const [allowFractional, setAllowFractional] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleOpenChange(next: boolean) {
    if (next) {
      setName(group?.name ?? "");
      setColor(group?.color ?? "blue");
      setAllowFractional(group?.allowFractional ?? false);
      setError(null);
    }
    setOpen(next);
  }

  function handleSave() {
    if (!name.trim()) return setError("Ingresá un nombre.");
    setError(null);
    startTransition(async () => {
      const result = await saveGroupAction({ name: name.trim(), color, allowFractional }, group?.id);
      if (result.status === "error") return setError(result.message);
      setOpen(false);
    });
  }

  function handleDelete() {
    if (!group) return;
    setError(null);
    startTransition(async () => {
      const result = await deleteGroupAction(group.id);
      if (result.status === "error") return setError(result.message);
      setOpen(false);
    });
  }

  return (
    <Sheet open={open} onOpenChange={handleOpenChange}>
      <SheetTrigger asChild>
        {isEdit ? (
          <Button variant="ghost" size="icon-sm" aria-label={`Editar grupo ${group.name}`}>
            <Pencil className="h-4 w-4" />
          </Button>
        ) : (
          <Button variant="outline">
            <Plus className="h-4 w-4" />
            Nuevo grupo
          </Button>
        )}
      </SheetTrigger>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>{isEdit ? "Editar grupo" : "Nuevo grupo"}</SheetTitle>
          <SheetDescription>Un tipo de inversión: oro, bonos, acciones...</SheetDescription>
        </SheetHeader>
        <div className="flex flex-col gap-4 p-4">
          <Field>
            <FieldLabel htmlFor="group-name">Nombre</FieldLabel>
            <Input
              id="group-name"
              value={name}
              maxLength={100}
              placeholder="Ej: Bonos a 20 años"
              onChange={(e) => setName(e.target.value)}
            />
          </Field>
          <Field>
            <FieldLabel>Color</FieldLabel>
            <div className="flex flex-wrap gap-2">
              {(Object.keys(GROUP_COLORS) as GroupColor[]).map((c) => (
                <button
                  key={c}
                  type="button"
                  aria-label={GROUP_COLORS[c].label}
                  aria-pressed={color === c}
                  title={GROUP_COLORS[c].label}
                  onClick={() => setColor(c)}
                  className={`size-8 rounded-full ${GROUP_COLORS[c].dot} ring-offset-2 ring-offset-background ${
                    color === c ? "ring-2 ring-foreground" : ""
                  }`}
                />
              ))}
            </div>
          </Field>
          <label className="flex items-start gap-3 rounded-lg border p-3 text-sm">
            <input
              type="checkbox"
              className="mt-0.5 size-4 accent-primary"
              checked={allowFractional}
              onChange={(e) => setAllowFractional(e.target.checked)}
            />
            <span className="flex flex-col gap-0.5">
              <span className="font-medium">Permite fracciones</span>
              <span className="text-muted-foreground">
                Para bonos. Si no, la calculadora de aporte sugiere unidades enteras.
              </span>
            </span>
          </label>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <Separator />
          <Button onClick={handleSave} disabled={isPending}>
            {isPending ? "Guardando..." : isEdit ? "Guardar cambios" : "Crear grupo"}
          </Button>
          {isEdit && (
            <Button
              variant="ghost"
              className="text-destructive"
              onClick={handleDelete}
              disabled={isPending || group.holdings.length > 0}
              title={group.holdings.length > 0 ? "Primero eliminá o mové sus inversiones" : undefined}
            >
              <Trash2 className="h-4 w-4" />
              Eliminar grupo
            </Button>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
