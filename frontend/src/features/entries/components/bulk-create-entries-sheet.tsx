"use client";

import { useActionState, useEffect, useState } from "react";
import { bulkCreateEntriesAction } from "@/src/features/entries/actions/bulk-create-entries";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/src/shared/components/ui/sheet";
import { Button } from "@/src/shared/components/ui/button";
import { Textarea } from "@/src/shared/components/ui/textarea";
import { Field, FieldDescription, FieldLabel } from "@/src/shared/components/ui/field";
import { Separator } from "@/src/shared/components/ui/separator";
import { ActionState } from "@/src/shared/types/action-state";
import { UploadCloud } from "lucide-react";

const initialState: ActionState = { status: "idle" };

const EXAMPLE = `[
  {
    "title": "Pago de alquiler",
    "description": "Alquiler julio",
    "date": "2026-07-01T00:00:00-03:00",
    "entryLines": [
      { "accountId": "<guid-cuenta-debe>", "amount": 1000, "type": "Debit" },
      { "accountId": "<guid-cuenta-haber>", "amount": 1000, "type": "Credit" }
    ]
  }
]`;

export default function BulkCreateEntriesSheet() {
  const [open, setOpen] = useState(false);
  const [jsonText, setJsonText] = useState("");
  const [state, formAction, pending] = useActionState(
    bulkCreateEntriesAction,
    initialState,
  );

  useEffect(() => {
    if (state.status === "success") {
      setOpen(false);
      setJsonText("");
    }
  }, [state]);

  const handleFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setJsonText(await file.text());
  };

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="outline">
          <UploadCloud className="h-4 w-4 mr-2" />
          Cargar asientos (JSON)
        </Button>
      </SheetTrigger>
      <SheetContent className="sm:max-w-xl overflow-y-auto md:min-w-125">
        <SheetHeader>
          <SheetTitle>Cargar asientos masivamente</SheetTitle>
          <SheetDescription>
            Subí o pegá un JSON con un array de asientos. Se validan todos
            antes de guardarse: si uno falla, no se guarda ninguno.
          </SheetDescription>
        </SheetHeader>
        <form action={formAction} className="flex flex-col gap-4 p-4">
          <input type="hidden" name="entriesJson" value={jsonText} />

          <Field>
            <FieldLabel htmlFor="entries-file">Archivo .json</FieldLabel>
            <input
              id="entries-file"
              type="file"
              accept=".json,application/json"
              onChange={handleFile}
            />
          </Field>

          <Field>
            <FieldLabel htmlFor="entries-json">O pegá el JSON acá</FieldLabel>
            <Textarea
              id="entries-json"
              value={jsonText}
              onChange={(e) => setJsonText(e.target.value)}
              placeholder={EXAMPLE}
              rows={14}
              className="font-mono text-xs"
            />
            <FieldDescription>
              Cada asiento tiene el mismo formato que POST /api/entry
              (title, description, date, entryLines con accountId, amount y
              type).
            </FieldDescription>
          </Field>

          {state.status === "error" && state.message && (
            <p className="text-sm text-destructive">{state.message}</p>
          )}

          <Separator />

          <Button type="submit" disabled={pending || !jsonText.trim()}>
            {pending ? "Cargando..." : "Cargar asientos"}
          </Button>
        </form>
      </SheetContent>
    </Sheet>
  );
}
