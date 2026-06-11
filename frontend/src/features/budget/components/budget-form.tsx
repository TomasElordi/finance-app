"use client";

import { useState, useTransition } from "react";
import { Account, NatureType } from "@/src/features/accounts/types/account";
import { Budget } from "../types/budget";
import { upsertBudgetsAction } from "../actions/upsert-budgets";
import { Button } from "@/src/shared/components/ui/button";
import { Input } from "@/src/shared/components/ui/input";
import { Save } from "lucide-react";

interface BudgetFormProps {
  accounts: Account[];
  budgets: Budget[];
  year: number;
  month: number;
}

export default function BudgetForm({ accounts, budgets, year, month }: BudgetFormProps) {
  const expenseAccounts = accounts.filter((a) => a.nature === String(NatureType.Expense) || Number(a.nature) === NatureType.Expense);

  const initialAmounts = Object.fromEntries(
    expenseAccounts.map((a) => {
      const existing = budgets.find((b) => b.accountId === a.id);
      return [a.id, existing ? String(existing.amount) : ""];
    }),
  );

  const [amounts, setAmounts] = useState<Record<string, string>>(initialAmounts);
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  function handleSave() {
    setError(null);
    setSuccess(false);

    const items = expenseAccounts
      .filter((a) => amounts[a.id] !== "" && Number(amounts[a.id]) > 0)
      .map((a) => ({
        accountId: a.id,
        year,
        month,
        amount: Number(amounts[a.id]),
      }));

    if (items.length === 0) return;

    startTransition(async () => {
      const result = await upsertBudgetsAction(items);
      if (result.status === "error") {
        setError(result.message);
      } else {
        setSuccess(true);
      }
    });
  }

  if (expenseAccounts.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        No tenés cuentas de tipo Egreso. Creá una desde la sección Cuentas.
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-2">
        {expenseAccounts.map((account) => (
          <div key={account.id} className="flex items-center gap-4 rounded-lg border bg-card px-4 py-3">
            <span className="flex-1 text-sm font-medium">{account.name}</span>
            <Input
              type="number"
              min="0"
              step="0.01"
              placeholder="0.00"
              className="w-36 text-right tabular-nums"
              value={amounts[account.id]}
              onChange={(e) => setAmounts((prev) => ({ ...prev, [account.id]: e.target.value }))}
            />
          </div>
        ))}
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}
      {success && <p className="text-sm text-green-600">Presupuesto guardado.</p>}

      <div className="flex justify-end">
        <Button onClick={handleSave} disabled={isPending}>
          <Save className="h-4 w-4 mr-2" />
          {isPending ? "Guardando..." : "Guardar presupuesto"}
        </Button>
      </div>
    </div>
  );
}
