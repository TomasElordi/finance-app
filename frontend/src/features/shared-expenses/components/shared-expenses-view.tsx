import { Receipt } from "lucide-react";
import PeriodSelector from "@/src/features/budget/components/period-selector";
import { SharedGroupDetail, SharedGroupSummary, SharedPeriod } from "../types/shared";
import BalanceSummary from "./balance-summary";
import ExpenseSheet from "./expense-sheet";
import ExpensesTable from "./expenses-table";
import GroupSettingsSheet from "./group-settings-sheet";
import GroupSwitcher from "./group-switcher";
import MonthSummary from "./month-summary";
import SettlementsList from "./settlements-list";
import { JoinGroupForm } from "./group-forms";

interface SharedExpensesViewProps {
  groups: SharedGroupSummary[];
  group: SharedGroupDetail;
  period: SharedPeriod;
  year: number;
  month: number;
  joinCode?: string;
}

export default function SharedExpensesView({
  groups,
  group,
  period,
  year,
  month,
  joinCode,
}: SharedExpensesViewProps) {
  // An invite link for a group the user isn't in yet.
  const pendingJoin = joinCode && joinCode.toUpperCase() !== group.inviteCode ? joinCode : undefined;

  return (
    <div className="p-6 flex flex-col gap-8">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-semibold">
            Gastos en común
            {groups.length < 2 && <span className="text-muted-foreground"> · {group.name}</span>}
          </h1>
          <GroupSwitcher groups={groups} groupId={group.id} />
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <PeriodSelector year={year} month={month} allowFuture />
          <GroupSettingsSheet group={group} />
          <ExpenseSheet group={group} year={year} month={month} />
        </div>
      </div>

      {pendingJoin && (
        <div className="flex max-w-md flex-col gap-3 rounded-lg border p-4">
          <h2 className="font-medium">Te invitaron a un grupo</h2>
          <JoinGroupForm initialCode={pendingJoin} />
        </div>
      )}

      <BalanceSummary group={group} year={year} month={month} />

      <section className="flex flex-col gap-3">
        <h2 className="text-sm font-semibold text-muted-foreground">Gastos del mes</h2>
        {period.expenses.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-4 rounded-lg border border-dashed py-16 text-center">
            <div className="rounded-full bg-muted p-4">
              <Receipt className="h-8 w-8 text-muted-foreground" />
            </div>
            <div className="flex flex-col gap-1">
              <p className="font-medium">No hay gastos en común este mes</p>
              <p className="text-sm text-muted-foreground">
                Cargá un gasto e indicá quién lo pagó y cómo se divide.
              </p>
            </div>
          </div>
        ) : (
          <>
            <ExpensesTable group={group} expenses={period.expenses} year={year} month={month} />
            <MonthSummary period={period} />
          </>
        )}
      </section>

      {period.settlements.length > 0 && (
        <SettlementsList groupId={group.id} settlements={period.settlements} />
      )}
    </div>
  );
}
