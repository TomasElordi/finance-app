import { CreditCard } from "lucide-react";
import { Account } from "@/src/features/accounts/types/account";
import PeriodSelector from "@/src/features/budget/components/period-selector";
import { CardStatement, CardUpcoming, Person } from "../types/card";
import CardItemSheet from "./card-item-sheet";
import CardItemsTable from "./card-items-table";
import PersonTotals from "./person-totals";
import UpcomingInstallments from "./upcoming-installments";

interface CardExpensesViewProps {
  statement: CardStatement;
  upcoming: CardUpcoming;
  accounts: Account[];
  people: Person[];
  year: number;
  month: number;
}

export default function CardExpensesView({
  statement,
  upcoming,
  accounts,
  people,
  year,
  month,
}: CardExpensesViewProps) {
  return (
    <div className="p-6 flex flex-col gap-8">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold">Tarjeta</h1>
        <div className="flex items-center gap-3">
          <PeriodSelector year={year} month={month} allowFuture />
          <CardItemSheet accounts={accounts} people={people} year={year} month={month} />
        </div>
      </div>

      {statement.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 py-24 text-center">
          <div className="rounded-full bg-muted p-4">
            <CreditCard className="h-8 w-8 text-muted-foreground" />
          </div>
          <div className="flex flex-col gap-1">
            <p className="font-medium">No hay ítems en el resumen de este mes</p>
            <p className="text-sm text-muted-foreground">
              Cargá los gastos, impuestos y saldos a favor de la tarjeta.
            </p>
          </div>
        </div>
      ) : (
        <>
          <CardItemsTable
            items={statement.items}
            accounts={accounts}
            people={people}
            year={year}
            month={month}
          />
          <section className="flex flex-col gap-3">
            <h2 className="text-sm font-semibold text-muted-foreground">Por persona</h2>
            <PersonTotals people={statement.people} total={statement.total} />
          </section>
        </>
      )}

      <UpcomingInstallments upcoming={upcoming} />
    </div>
  );
}
