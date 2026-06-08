"use client";

import { Account } from "@/src/features/accounts/types/account";
import { getNatureOrder } from "../utils/nature";
import CreateAccountSheet from "./create-account-sheet";
import AccountCard from "./account-card";

export default function AccountsView({ accounts }: { accounts: Account[] }) {
  const sorted = [...accounts].sort((a, b) => {
    const orderDiff = getNatureOrder(a.nature) - getNatureOrder(b.nature);
    return orderDiff !== 0 ? orderDiff : a.name.localeCompare(b.name, "es");
  });

  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Cuentas</h1>
        <CreateAccountSheet />
      </div>

      {accounts.length === 0 ? (
        <p className="text-muted-foreground text-sm">No tenés cuentas aún.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {sorted.map((account) => (
            <AccountCard key={account.id} account={account} />
          ))}
        </div>
      )}
    </div>
  );
}
