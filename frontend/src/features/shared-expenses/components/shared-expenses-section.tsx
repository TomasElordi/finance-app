import { getSharedGroups } from "../data/get-shared-groups";
import { getSharedGroup } from "../data/get-shared-group";
import { getSharedPeriod } from "../data/get-shared-period";
import { getSharedTrend } from "../data/get-shared-trend";
import SharedExpensesView from "./shared-expenses-view";
import SharedExpensesOnboarding from "./shared-expenses-onboarding";

interface SharedExpensesSectionProps {
  groupId?: string;
  year: number;
  month: number;
  joinCode?: string;
}

export default async function SharedExpensesSection({
  groupId,
  year,
  month,
  joinCode,
}: SharedExpensesSectionProps) {
  const groups = await getSharedGroups();
  if (groups.length === 0) {
    return <SharedExpensesOnboarding joinCode={joinCode} />;
  }

  // Unknown or missing ?group falls back to the first group.
  const selectedId = groups.some((g) => g.id === groupId) ? groupId! : groups[0].id;
  const [group, period, trend] = await Promise.all([
    getSharedGroup(selectedId),
    getSharedPeriod(selectedId, year, month),
    getSharedTrend(selectedId, year, month),
  ]);
  if (!group) return <SharedExpensesOnboarding joinCode={joinCode} />;

  return (
    <SharedExpensesView
      groups={groups}
      group={group}
      period={period}
      trend={trend}
      year={year}
      month={month}
      joinCode={joinCode}
    />
  );
}
