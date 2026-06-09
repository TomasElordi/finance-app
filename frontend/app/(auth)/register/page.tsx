import { Suspense } from "react";
import RegisterCard from "@/src/features/auth/register/components/register-card";
import AuthCardSkeleton from "@/src/features/auth/components/auth-card-skeleton";

export default function RegisterPage() {
  return (
    <div className="min-h-screen flex justify-center items-center">
      <Suspense fallback={<AuthCardSkeleton fields={4} />}>
        <RegisterCard />
      </Suspense>
    </div>
  );
}
