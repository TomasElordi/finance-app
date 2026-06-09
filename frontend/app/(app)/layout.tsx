import { Suspense } from "react";
import TriggerSidebar from "@/src/shared/components/trigger-sidebar";
import AppSidebarWrapper from "@/src/shared/components/app-sidebar-wrapper";
import AppSidebarSkeleton from "@/src/shared/components/app-sidebar-skeleton";
import { SidebarProvider } from "@/src/shared/components/ui/sidebar";

export default function LayoutApp({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div suppressHydrationWarning={true}>
      <SidebarProvider>
        <Suspense fallback={<AppSidebarSkeleton />}>
          <AppSidebarWrapper />
        </Suspense>
        <main className="flex-1 overflow-auto">
          <header className="sticky top-0 z-10 flex items-center px-4 py-3 bg-background border-b border-border md:hidden">
            <TriggerSidebar />
          </header>
          {children}
        </main>
      </SidebarProvider>
    </div>
  );
}
