import { session } from "@/src/shared/lib/session";
import { AppSidebar } from "./ui/app-sidebar";

export default async function AppSidebarWrapper() {
  const userName = await session.getUserName();
  return <AppSidebar userName={userName ?? "Usuario"} />;
}
