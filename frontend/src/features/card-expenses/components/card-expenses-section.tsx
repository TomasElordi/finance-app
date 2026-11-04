import { getAccounts } from "@/src/features/accounts/data/get-accounts";
import { getCardStatement } from "../data/get-card-statement";
import { getCardUpcoming } from "../data/get-card-upcoming";
import { getPeople } from "../data/get-people";
import CardExpensesView from "./card-expenses-view";

interface CardExpensesSectionProps {
  year: number;
  month: number;
}

export default async function CardExpensesSection({ year, month }: CardExpensesSectionProps) {
  const [statement, upcoming, accounts, people] = await Promise.all([
    getCardStatement(year, month),
    getCardUpcoming(year, month),
    getAccounts(),
    getPeople(),
  ]);

  return (
    <CardExpensesView
      statement={statement}
      upcoming={upcoming}
      accounts={accounts}
      people={people}
      year={year}
      month={month}
    />
  );
}
