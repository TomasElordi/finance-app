"use client";

import { formatCurrency } from "@/src/features/accounts/utils/format";
import { SharedExpense, SharedGroupDetail } from "../types/shared";
import { deleteExpenseAction } from "../actions/expenses";
import { formatShortDate } from "../utils/split";
import ConfirmDeleteButton from "./confirm-delete-button";
import ExpenseSheet from "./expense-sheet";

interface ExpensesTableProps {
  group: SharedGroupDetail;
  expenses: SharedExpense[];
  year: number;
  month: number;
}

function splitLabel(expense: SharedExpense) {
  return expense.splits
    .map((s) => `${s.name} ${Math.round((s.amount / expense.total) * 100)}%`)
    .join(" · ");
}

export default function ExpensesTable({ group, expenses, year, month }: ExpensesTableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm">
        <thead className="bg-muted/50 text-left text-muted-foreground">
          <tr>
            <th className="px-3 py-2 font-medium">Fecha</th>
            <th className="px-3 py-2 font-medium">Descripción</th>
            <th className="hidden px-3 py-2 font-medium md:table-cell">Categoría</th>
            <th className="px-3 py-2 font-medium">Pagó</th>
            <th className="hidden px-3 py-2 font-medium lg:table-cell">División</th>
            <th className="px-3 py-2 text-right font-medium">Total</th>
            <th className="w-20 px-3 py-2" />
          </tr>
        </thead>
        <tbody>
          {expenses.map((e) => (
            <tr key={e.id} className="border-t">
              <td className="px-3 py-2 tabular-nums text-muted-foreground">{formatShortDate(e.date)}</td>
              <td className="px-3 py-2">
                <div className="flex flex-col">
                  <span>{e.description}</span>
                  <span className="text-xs text-muted-foreground lg:hidden">{splitLabel(e)}</span>
                </div>
              </td>
              <td className="hidden px-3 py-2 text-muted-foreground md:table-cell">
                {e.categoryName ?? "—"}
              </td>
              <td className="px-3 py-2">{e.paidByName}</td>
              <td className="hidden px-3 py-2 text-muted-foreground lg:table-cell">
                <div className="flex flex-col">
                  {e.splits.map((s) => (
                    <span key={s.userId} className="tabular-nums">
                      {s.name}: {formatCurrency(s.amount)}
                      <span className="text-xs"> ({Math.round((s.amount / e.total) * 100)}%)</span>
                    </span>
                  ))}
                </div>
              </td>
              <td className="px-3 py-2 text-right tabular-nums">{formatCurrency(e.total)}</td>
              <td className="px-3 py-2">
                <div className="flex justify-end gap-1">
                  <ExpenseSheet group={group} year={year} month={month} expense={e} />
                  <ConfirmDeleteButton
                    title={`¿Eliminar "${e.description}"?`}
                    description="Se elimina para todos los miembros del grupo. Esta acción no se puede deshacer."
                    onConfirm={() => deleteExpenseAction(group.id, e.id)}
                  />
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
