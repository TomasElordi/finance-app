import { BudgetSummaryItem } from "../types/period-summary";
import BudgetBarChart from "./budget-bar-chart";
import BudgetProgressCards from "./budget-progress-cards";

export default function BudgetCharts({ items }: { items: BudgetSummaryItem[] }) {
  return (
    <div className="flex flex-col gap-8">
      <div>
        <h2 className="text-lg font-semibold mb-4">Presupuestado vs Real</h2>
        <BudgetBarChart items={items} />
      </div>
      <div>
        <h2 className="text-lg font-semibold mb-4">Progreso por cuenta</h2>
        <BudgetProgressCards items={items} />
      </div>
    </div>
  );
}
