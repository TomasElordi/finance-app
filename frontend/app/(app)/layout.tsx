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
        <main className="flex-1 overflow-auto">{children}</main>
      </SidebarProvider>
    </div>
  );
}
