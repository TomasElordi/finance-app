import { Account } from "@/src/features/accounts/types/account";
import CreateAccountSheet from "./create-account-sheet";
import AccountsEmptyState from "./accounts-empty-state";
import AccountList from "./account-list";

export default function AccountsView({ accounts }: { accounts: Account[] }) {
  return (
    <div className="p-6 flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Cuentas</h1>
        <CreateAccountSheet />
      </div>

      {accounts.length === 0 ? (
        <AccountsEmptyState />
      ) : (
        <AccountList accounts={accounts} />
      )}
    </div>
  );
}
