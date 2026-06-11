"use client";

import { useState, useTransition } from "react";
import { Account } from "@/src/features/accounts/types/account";
import { Budget } from "../types/budget";
import { upsertBudgetsAction } from "../actions/upsert-budgets";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/src/shared/components/ui/sheet";
import { Button } from "@/src/shared/components/ui/button";
import { Input } from "@/src/shared/components/ui/input";
import { Separator } from "@/src/shared/components/ui/separator";
import { Pencil, Plus } from "lucide-react";

interface BudgetSheetProps {
  accounts: Account[];
  budgets: Budget[];
  year: number;
  month: number;
  hasExistingBudgets: boolean;
}

export default function BudgetSheet({
  accounts,
  budgets,
  year,
  month,
  hasExistingBudgets,
}: BudgetSheetProps) {
  const expenseAccounts = accounts.filter((a) => a.nature === "Expense");

  const initialAmounts = Object.fromEntries(
    expenseAccounts.map((a) => {
      const existing = budgets.find((b) => b.accountId === a.id);
      return [a.id, existing ? String(existing.amount) : ""];
    }),
  );

  const [open, setOpen] = useState(false);
  const [amounts, setAmounts] = useState<Record<string, string>>(initialAmounts);
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  function handleSave() {
    setError(null);

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
        setOpen(false);
      }
    });
  }

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        {hasExistingBudgets ? (
          <Button variant="outline" size="sm">
            <Pencil className="h-4 w-4 mr-2" />
            Editar presupuesto
          </Button>
        ) : (
          <Button size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Definir presupuesto
          </Button>
        )}
      </SheetTrigger>
      <SheetContent className="overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Presupuesto mensual</SheetTitle>
        </SheetHeader>
        <div className="flex flex-col gap-3 p-4">
          {expenseAccounts.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No tenés cuentas de tipo Egreso. Creá una desde la sección Cuentas.
            </p>
          ) : (
            <>
              {expenseAccounts.map((account) => (
                <div
                  key={account.id}
                  className="flex items-center gap-4 rounded-lg border bg-card px-4 py-3"
                >
                  <span className="flex-1 text-sm font-medium">{account.name}</span>
                  <Input
                    type="number"
                    min="0"
                    step="0.01"
                    placeholder="0.00"
                    className="w-32 text-right tabular-nums"
                    value={amounts[account.id]}
                    onChange={(e) =>
                      setAmounts((prev) => ({ ...prev, [account.id]: e.target.value }))
                    }
                  />
                </div>
              ))}

              {error && <p className="text-sm text-destructive">{error}</p>}

              <Separator />

              <Button onClick={handleSave} disabled={isPending}>
                {isPending ? "Guardando..." : "Guardar"}
              </Button>
            </>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
