import { Repeat } from "lucide-react";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { Account } from "@/src/features/accounts/types/account";
import { CARD_ITEM_TYPE_LABELS, CardItem, Person } from "../types/card";
import CardItemSheet from "./card-item-sheet";
import DeleteCardItemButton from "./delete-card-item-button";

interface CardItemsTableProps {
  items: CardItem[];
  accounts: Account[];
  people: Person[];
  year: number;
  month: number;
}

export function signedAmount(item: CardItem) {
  return item.type === "Credit" ? -item.amount : item.amount;
}

export default function CardItemsTable({ items, accounts, people, year, month }: CardItemsTableProps) {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm">
        <thead className="bg-muted/50 text-left text-muted-foreground">
          <tr>
            <th className="px-3 py-2 font-medium">Nombre</th>
            <th className="px-3 py-2 font-medium">Persona</th>
            <th className="hidden px-3 py-2 font-medium md:table-cell">Cuenta</th>
            <th className="px-3 py-2 font-medium">Cuota</th>
            <th className="px-3 py-2 text-right font-medium">Monto</th>
            <th className="w-20 px-3 py-2" />
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.id} className="border-t">
              <td className="px-3 py-2">
                <div className="flex flex-col">
                  <span>{item.name}</span>
                  {item.type !== "Expense" && (
                    <span className="text-xs text-muted-foreground">
                      {CARD_ITEM_TYPE_LABELS[item.type]}
                    </span>
                  )}
                </div>
              </td>
              <td className="px-3 py-2">{item.personName}</td>
              <td className="hidden px-3 py-2 text-muted-foreground md:table-cell">
                {item.accountName ?? "—"}
              </td>
              <td className="px-3 py-2 tabular-nums text-muted-foreground">
                {item.isRecurring ? (
                  <span className="inline-flex items-center gap-1">
                    <Repeat className="size-3.5" />
                    Mensual
                  </span>
                ) : item.installments > 1 ? (
                  `${item.installmentNumber}/${item.installments}`
                ) : (
                  "—"
                )}
              </td>
              <td
                className={`px-3 py-2 text-right tabular-nums ${
                  item.type === "Credit" ? "text-green-600 dark:text-green-500" : ""
                }`}
              >
                {formatCurrency(signedAmount(item))}
              </td>
              <td className="px-3 py-2">
                <div className="flex justify-end gap-1">
                  <CardItemSheet
                    accounts={accounts}
                    people={people}
                    year={year}
                    month={month}
                    item={item}
                  />
                  <DeleteCardItemButton item={item} year={year} month={month} />
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
