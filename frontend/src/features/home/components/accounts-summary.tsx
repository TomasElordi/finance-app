import { Account } from "@/src/features/accounts/types/account";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import {
  getNatureLabel,
  getNatureColor,
  getNatureOrder,
} from "@/src/features/accounts/utils/nature";
import { cn } from "@/src/shared/lib/utils";

interface AccountsSummaryProps {
  accounts: Account[];
}

export default function AccountsSummary({ accounts }: AccountsSummaryProps) {
  const grouped = accounts.reduce<Record<string, Account[]>>((acc, account) => {
    const key = account.nature;
    if (!acc[key]) acc[key] = [];
    acc[key].push(account);
    return acc;
  }, {});

  const sortedNatures = Object.keys(grouped).sort(
    (a, b) => getNatureOrder(a) - getNatureOrder(b),
  );

  return (
    <div className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
        Cuentas
      </h2>
      <div className="flex flex-col gap-4">
        {sortedNatures.map((nature) => (
          <div key={nature} className="flex flex-col gap-1.5">
            <span
              className={cn(
                "text-xs font-medium px-2 py-0.5 rounded-full w-fit",
                getNatureColor(nature),
              )}
            >
              {getNatureLabel(nature)}
            </span>
            <div className="flex flex-col gap-1">
              {grouped[nature]
                .sort((a, b) => a.name.localeCompare(b.name, "es"))
                .map((account) => (
                  <div
                    key={account.id}
                    className="flex items-center justify-between rounded-md px-3 py-2 border text-sm"
                  >
                    <span className="text-muted-foreground">{account.name}</span>
                    <span className="font-medium tabular-nums">
                      {formatCurrency(account.balance)}
                    </span>
                  </div>
                ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
