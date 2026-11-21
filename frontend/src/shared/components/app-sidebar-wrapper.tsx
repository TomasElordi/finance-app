import { session } from "@/src/shared/lib/session";
import { apiBaseUrl } from "@/src/shared/lib/envs";
import { AppSidebar } from "./ui/app-sidebar";

// The MCP endpoint lives at the API origin root (/mcp), not under /api.
const mcpUrl = apiBaseUrl ? new URL("/mcp", apiBaseUrl).toString() : "/mcp";

export default async function AppSidebarWrapper() {
  const userName = await session.getUserName();
  return <AppSidebar userName={userName ?? "Usuario"} mcpUrl={mcpUrl} />;
}
