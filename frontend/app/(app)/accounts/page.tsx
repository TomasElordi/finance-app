import { Suspense } from "react";
import AccountsSection from "@/src/features/accounts/components/accounts-section";
import AccountsSkeleton from "@/src/features/accounts/components/accounts-skeleton";

export default function AccountsPage() {
  return (
    <Suspense fallback={<AccountsSkeleton />}>
      <AccountsSection />
    </Suspense>
  );
}
