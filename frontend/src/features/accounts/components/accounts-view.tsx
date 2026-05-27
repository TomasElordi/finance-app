"use client";

import { Account } from "@/src/features/accounts/types/account";
import CreateAccountSheet from "./create-account-sheet";
import DeleteAccountForm from "./delete-account-form";

export default function AccountsView({ accounts }: { accounts: Account[] }) {
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
          {accounts.map((account) => (
            <div
              key={account.id}
              className="flex items-center justify-between rounded-lg border px-4 py-3"
            >
              <div className="flex flex-col gap-0.5">
                <span className="font-medium">{account.name}</span>
                <span className="text-xs text-muted-foreground">
                  {account.nature}
                </span>
              </div>
              <DeleteAccountForm id={account.id} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
