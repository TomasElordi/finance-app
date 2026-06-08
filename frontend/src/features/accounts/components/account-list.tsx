import { Account } from "../types/account";
import { getNatureOrder } from "../utils/nature";
import AccountCard from "./account-card";

export default function AccountList({ accounts }: { accounts: Account[] }) {
  const sorted = [...accounts].sort((a, b) => {
    const orderDiff = getNatureOrder(a.nature) - getNatureOrder(b.nature);
    return orderDiff !== 0 ? orderDiff : a.name.localeCompare(b.name, "es");
  });

  return (
    <div className="flex flex-col gap-2">
      {sorted.map((account) => (
        <AccountCard key={account.id} account={account} />
      ))}
    </div>
  );
}
