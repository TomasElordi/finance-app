import TriggerSidebar from "@/src/shared/components/trigger-sidebar";
import { AppSidebar } from "@/src/shared/components/ui/app-sidebar";
import { SidebarProvider } from "@/src/shared/components/ui/sidebar";
import { session } from "@/src/shared/lib/session";

export default async function LayoutApp({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const userName = await session.getUserName();

  return (
    <div suppressHydrationWarning={true}>
      <SidebarProvider>
        <AppSidebar userName={userName ?? "Usuario"} />
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
