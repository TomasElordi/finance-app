import { Account } from "@/src/features/accounts/types/account";
import { getNatureColor, getNatureLabel } from "../utils/nature";
import { formatCurrency } from "../utils/format";
import DeleteAccountForm from "./delete-account-form";

export default function AccountCard({ account }: { account: Account }) {
  return (
    <div className="flex items-center justify-between rounded-lg border bg-card px-4 py-3 hover:bg-muted/30 transition-colors">
      <div className="flex flex-col gap-1">
        <span className="font-medium leading-none">{account.name}</span>
        <span
          className={`text-xs font-medium px-1.5 py-0.5 rounded-full w-fit ${getNatureColor(account.nature)}`}
        >
          {getNatureLabel(account.nature)}
        </span>
      </div>
      <div className="flex items-center gap-3">
        <span className="text-sm font-semibold tabular-nums">
          {formatCurrency(account.balance)}
        </span>
        <DeleteAccountForm id={account.id} />
      </div>
    </div>
  );
}
