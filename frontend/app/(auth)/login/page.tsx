import { Suspense } from "react";
import LoginCard from "@/src/features/auth/login/components/login-card";
import AuthCardSkeleton from "@/src/features/auth/components/auth-card-skeleton";

export default function LoginPage() {
  return (
    <div className="min-h-screen flex justify-center items-center">
      <Suspense fallback={<AuthCardSkeleton fields={2} />}>
        <LoginCard />
      </Suspense>
    </div>
  );
}
