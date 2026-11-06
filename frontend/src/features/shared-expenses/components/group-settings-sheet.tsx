"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { Check, Copy, RefreshCw, Settings, X } from "lucide-react";
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
import AlertConfirm from "@/src/shared/components/alert-confirm";
import { SharedGroupDetail } from "../types/shared";
import { leaveGroupAction, regenerateInviteCodeAction, renameGroupAction } from "../actions/groups";
import { createCategoryAction, deleteCategoryAction } from "../actions/categories";
import { CreateGroupForm, JoinGroupForm } from "./group-forms";

export default function GroupSettingsSheet({ group }: { group: SharedGroupDetail }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [name, setName] = useState(group.name);
  const [newCategory, setNewCategory] = useState("");
  const [copied, setCopied] = useState<"code" | "link" | null>(null);
  const [confirm, setConfirm] = useState<"leave" | "regenerate" | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const me = group.members.find((m) => m.isMe);
  const isLastMember = group.members.length === 1;

  function handleOpenChange(next: boolean) {
    if (next) {
      setName(group.name);
      setNewCategory("");
      setError(null);
    }
    setOpen(next);
  }

  function run(action: () => Promise<{ status: "success" } | { status: "error"; message: string }>, after?: () => void) {
    setError(null);
    startTransition(async () => {
      const result = await action();
      if (result.status === "error") {
        setError(result.message);
        return;
      }
      after?.();
    });
  }

  async function copy(kind: "code" | "link") {
    const text =
      kind === "code"
        ? group.inviteCode
        : `${window.location.origin}/shared-expenses?join=${group.inviteCode}`;
    try {
      await navigator.clipboard.writeText(text);
      setCopied(kind);
      setTimeout(() => setCopied(null), 1500);
    } catch {
      setError("No se pudo copiar, copialo a mano.");
    }
  }

  function addCategory() {
    const value = newCategory.trim();
    if (!value) return;
    run(() => createCategoryAction(group.id, value), () => setNewCategory(""));
  }

  return (
    <>
      <AlertConfirm
        open={confirm === "regenerate"}
        title="¿Generar un código nuevo?"
        description="El código actual deja de funcionar. Los miembros actuales no se ven afectados."
        confirmText="Generar"
        onCancel={() => setConfirm(null)}
        onConfirm={() => {
          setConfirm(null);
          run(() => regenerateInviteCodeAction(group.id));
        }}
      />
      <AlertConfirm
        open={confirm === "leave"}
        title={isLastMember ? `¿Eliminar ${group.name}?` : `¿Salir de ${group.name}?`}
        description={
          isLastMember
            ? "Sos el único miembro: el grupo y todos sus gastos se eliminan definitivamente."
            : "Dejás de ver el grupo. Solo podés salir si tu saldo está en cero; tus gastos pasados se mantienen para los demás."
        }
        confirmText={isLastMember ? "Eliminar grupo" : "Salir"}
        onCancel={() => setConfirm(null)}
        onConfirm={() => {
          setConfirm(null);
          run(
            () => leaveGroupAction(group.id),
            () => {
              setOpen(false);
              router.push("/shared-expenses");
            },
          );
        }}
      />

      <Sheet open={open} onOpenChange={handleOpenChange}>
        <SheetTrigger asChild>
          <Button variant="outline" size="icon" aria-label="Configurar grupo">
            <Settings className="h-4 w-4" />
          </Button>
        </SheetTrigger>
        <SheetContent className="overflow-y-auto sm:max-w-md">
          <SheetHeader>
            <SheetTitle>{group.name}</SheetTitle>
            <SheetDescription>
              {group.members.map((m) => m.name).join(", ")}
            </SheetDescription>
          </SheetHeader>

          <div className="flex flex-col gap-5 p-4">
            <section className="flex flex-col gap-2">
              <h3 className="text-sm font-medium">Invitar personas</h3>
              <p className="text-sm text-muted-foreground">
                Compartí el código o el link. Quien lo use tiene que tener cuenta en la app.
              </p>
              <div className="flex items-center gap-2">
                <code className="flex-1 rounded-md bg-muted px-3 py-2 text-center font-mono text-lg tracking-widest">
                  {group.inviteCode}
                </code>
                <Button variant="outline" size="icon" aria-label="Copiar código" onClick={() => copy("code")}>
                  {copied === "code" ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  aria-label="Generar código nuevo"
                  disabled={isPending}
                  onClick={() => setConfirm("regenerate")}
                >
                  <RefreshCw className="h-4 w-4" />
                </Button>
              </div>
              <Button variant="secondary" size="sm" onClick={() => copy("link")}>
                {copied === "link" ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                Copiar link de invitación
              </Button>
            </section>

            <Separator />

            <section className="flex flex-col gap-2">
              <h3 className="text-sm font-medium">Categorías</h3>
              <div className="flex flex-wrap gap-2">
                {group.categories.map((c) => (
                  <span
                    key={c.id}
                    className="inline-flex items-center gap-1 rounded-full border py-0.5 pr-1 pl-3 text-sm"
                  >
                    {c.name}
                    <button
                      type="button"
                      aria-label={`Eliminar ${c.name}`}
                      className="rounded-full p-0.5 text-muted-foreground hover:bg-muted hover:text-foreground"
                      disabled={isPending}
                      onClick={() => run(() => deleteCategoryAction(group.id, c.id))}
                    >
                      <X className="size-3.5" />
                    </button>
                  </span>
                ))}
                {group.categories.length === 0 && (
                  <span className="text-sm text-muted-foreground">Sin categorías.</span>
                )}
              </div>
              <div className="flex gap-2">
                <Input
                  placeholder="Nueva categoría"
                  maxLength={100}
                  value={newCategory}
                  onChange={(e) => setNewCategory(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      addCategory();
                    }
                  }}
                />
                <Button variant="outline" onClick={addCategory} disabled={isPending || !newCategory.trim()}>
                  Agregar
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                Al eliminar una categoría sus gastos quedan sin categoría.
              </p>
            </section>

            <Separator />

            <section className="flex flex-col gap-2">
              <Field>
                <FieldLabel htmlFor="group-name">Nombre del grupo</FieldLabel>
                <div className="flex gap-2">
                  <Input
                    id="group-name"
                    maxLength={100}
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                  />
                  <Button
                    variant="outline"
                    disabled={isPending || !name.trim() || name.trim() === group.name}
                    onClick={() => run(() => renameGroupAction(group.id, name.trim()))}
                  >
                    Guardar
                  </Button>
                </div>
              </Field>
            </section>

            {error && <p className="text-sm text-destructive">{error}</p>}

            <Separator />

            <details className="flex flex-col gap-3">
              <summary className="cursor-pointer text-sm font-medium">Otro grupo</summary>
              <div className="mt-3 flex flex-col gap-4">
                <CreateGroupForm onDone={() => setOpen(false)} />
                <JoinGroupForm onDone={() => setOpen(false)} />
              </div>
            </details>

            <Separator />

            <Button
              variant="ghost"
              className="text-destructive"
              disabled={isPending || (!isLastMember && (me?.balance ?? 0) !== 0)}
              onClick={() => setConfirm("leave")}
            >
              {isLastMember ? "Eliminar grupo" : "Salir del grupo"}
            </Button>
            {!isLastMember && (me?.balance ?? 0) !== 0 && (
              <p className="-mt-3 text-center text-xs text-muted-foreground">
                Para salir, primero saldá tu saldo.
              </p>
            )}
          </div>
        </SheetContent>
      </Sheet>
    </>
  );
}
