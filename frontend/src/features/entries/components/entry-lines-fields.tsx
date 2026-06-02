"use client";

import { useState } from "react";
import { Account } from "@/src/features/accounts/types/account";
import { EntryLine } from "@/src/features/entries/types/entry";
import { Input } from "@/src/shared/components/ui/input";
import { Button } from "@/src/shared/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import { Plus, Trash2 } from "lucide-react";

type DraftLine = {
  accountId: string;
  amount: string;
  type: "Credit" | "Debit";
};

function toDraft(line: EntryLine): DraftLine {
  return {
    accountId: line.accountId,
    amount: String(line.amount),
    type: line.type,
  };
}

const DEFAULT_LINES: DraftLine[] = [
  { accountId: "", amount: "", type: "Debit" },
  { accountId: "", amount: "", type: "Credit" },
];

interface EntryLinesFieldsProps {
  accounts: Account[];
  initialLines?: EntryLine[];
}

export default function EntryLinesFields({
  accounts,
  initialLines,
}: EntryLinesFieldsProps) {
  const [lines, setLines] = useState<DraftLine[]>(
    initialLines ? initialLines.map(toDraft) : DEFAULT_LINES,
  );

  const totalDebits = lines.reduce(
    (sum, l) => (l.type === "Debit" ? sum + (parseFloat(l.amount) || 0) : sum),
    0,
  );
  const totalCredits = lines.reduce(
    (sum, l) =>
      l.type === "Credit" ? sum + (parseFloat(l.amount) || 0) : sum,
    0,
  );
  const isBalanced =
    Math.round(totalDebits * 100) === Math.round(totalCredits * 100) &&
    totalDebits > 0;

  function updateLine(index: number, field: keyof DraftLine, value: string) {
    setLines((prev) =>
      prev.map((l, i) => (i === index ? { ...l, [field]: value } : l)),
    );
  }

  function addLine() {
    setLines((prev) => [
      ...prev,
      { accountId: "", amount: "", type: "Credit" },
    ]);
  }

  function removeLine(index: number) {
    if (lines.length <= 2) return;
    setLines((prev) => prev.filter((_, i) => i !== index));
  }

  return (
    <div className="flex flex-col gap-3">
      <div className="flex flex-col gap-2">
        {lines.map((line, i) => (
          <div key={i} className="flex gap-2 items-center">
            {/* Hidden inputs for FormData submission */}
            <input
              type="hidden"
              name={`entryLines.${i}.accountId`}
              value={line.accountId}
            />
            <input
              type="hidden"
              name={`entryLines.${i}.amount`}
              value={line.amount}
            />
            <input
              type="hidden"
              name={`entryLines.${i}.type`}
              value={line.type}
            />

            <Select
              value={line.accountId}
              onValueChange={(v) => updateLine(i, "accountId", v)}
            >
              <SelectTrigger className="flex-1 w-full">
                <SelectValue placeholder="Seleccionar cuenta" />
              </SelectTrigger>
              <SelectContent>
                {accounts.map((a) => (
                  <SelectItem key={a.id} value={a.id}>
                    {a.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Input
              type="number"
              step="0.01"
              min="0.01"
              placeholder="0.00"
              value={line.amount}
              onChange={(e) => updateLine(i, "amount", e.target.value)}
              className="w-28"
            />

            <Select
              value={line.type}
              onValueChange={(v) =>
                updateLine(i, "type", v as "Credit" | "Debit")
              }
            >
              <SelectTrigger className="w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Debit">Débito</SelectItem>
                <SelectItem value="Credit">Crédito</SelectItem>
              </SelectContent>
            </Select>

            <Button
              type="button"
              variant="ghost"
              size="icon-sm"
              onClick={() => removeLine(i)}
              disabled={lines.length <= 2}
              aria-label="Eliminar línea"
            >
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </div>
        ))}
      </div>

      <Button type="button" variant="outline" size="sm" onClick={addLine}>
        <Plus className="h-4 w-4" />
        Agregar línea
      </Button>

      <div
        className={`flex items-center justify-between rounded-md border px-3 py-2 text-sm ${
          isBalanced
            ? "border-green-500/30 bg-green-500/10 text-green-600"
            : "border-destructive/30 bg-destructive/10 text-destructive"
        }`}
      >
        <span>
          Débitos: ${totalDebits.toFixed(2)} | Créditos: $
          {totalCredits.toFixed(2)}
        </span>
        <span className="font-medium">
          {isBalanced ? "Balanceado" : "Desbalanceado"}
        </span>
      </div>
    </div>
  );
}
