"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/src/shared/components/ui/button";
import { Input } from "@/src/shared/components/ui/input";
import { Field, FieldLabel } from "@/src/shared/components/ui/field";
import { createGroupAction, joinGroupAction } from "../actions/groups";

function useGroupForm(
  submit: (value: string) => ReturnType<typeof createGroupAction>,
  onDone?: () => void,
) {
  const router = useRouter();
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function run(value: string) {
    if (!value.trim()) return;
    setError(null);
    startTransition(async () => {
      const result = await submit(value.trim());
      if (result.status === "error") {
        setError(result.message);
        return;
      }
      onDone?.();
      router.push(`/shared-expenses?group=${result.data.id}`);
    });
  }

  return { error, isPending, run };
}

export function CreateGroupForm({ onDone }: { onDone?: () => void }) {
  const [name, setName] = useState("");
  const { error, isPending, run } = useGroupForm(createGroupAction, onDone);

  return (
    <form
      className="flex flex-col gap-3"
      onSubmit={(e) => {
        e.preventDefault();
        run(name);
      }}
    >
      <Field>
        <FieldLabel htmlFor="new-group-name">Nombre del grupo</FieldLabel>
        <Input
          id="new-group-name"
          placeholder="Ej: Casa"
          value={name}
          maxLength={100}
          onChange={(e) => setName(e.target.value)}
        />
      </Field>
      {error && <p className="text-sm text-destructive">{error}</p>}
      <Button type="submit" disabled={isPending || !name.trim()}>
        {isPending ? "Creando..." : "Crear grupo"}
      </Button>
    </form>
  );
}

export function JoinGroupForm({ initialCode, onDone }: { initialCode?: string; onDone?: () => void }) {
  const [code, setCode] = useState(initialCode ?? "");
  const { error, isPending, run } = useGroupForm(joinGroupAction, onDone);

  return (
    <form
      className="flex flex-col gap-3"
      onSubmit={(e) => {
        e.preventDefault();
        run(code);
      }}
    >
      <Field>
        <FieldLabel htmlFor="join-group-code">Código de invitación</FieldLabel>
        <Input
          id="join-group-code"
          placeholder="Ej: K7M2QX9P"
          value={code}
          maxLength={16}
          className="font-mono uppercase tracking-widest"
          onChange={(e) => setCode(e.target.value.toUpperCase())}
        />
      </Field>
      {error && <p className="text-sm text-destructive">{error}</p>}
      <Button type="submit" variant="outline" disabled={isPending || !code.trim()}>
        {isPending ? "Uniéndote..." : "Unirme al grupo"}
      </Button>
    </form>
  );
}
