"use client";
import { createContext, useContext, useMemo } from "react";
import { Account } from "../../accounts/types/account";

const AccountsContext = createContext<{
  accounts: Account[];
  accountsMap: Record<string, string>;
}>({ accounts: [], accountsMap: {} });

export function AccountProvider({
  accounts,
  children,
}: {
  accounts: Account[];
  children: React.ReactNode;
}) {
  const accountsMap = useMemo(
    () => Object.fromEntries(accounts.map((a: Account) => [a.id, a.name])),
    [accounts],
  );
  return (
    <AccountsContext.Provider value={{ accounts, accountsMap }}>
      {children}
    </AccountsContext.Provider>
  );
}

export const useAccounts = () => useContext(AccountsContext);
