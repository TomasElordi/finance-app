"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import type { LucideIcon } from "lucide-react";
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "./sidebar";
import {
  BarChart2,
  BookOpen,
  ChevronsUpDown,
  CircleDollarSign,
  CreditCard,
  House,
  LogOut,
  Plug,
  Receipt,
  Target,
  User2,
  Users,
  Wallet,
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "./dropdown-menu";
import { logoutAction } from "@/src/features/auth/logout/actions/logout";
import { ConnectMcpSheet } from "../connect-mcp-sheet";

type NavItem = { href: string; label: string; icon: LucideIcon };

const HOME_ITEM: NavItem = { href: "/home", label: "Inicio", icon: House };

// Grouped by how the modules relate: accounting is the source of truth,
// analysis reads from it, and tools keep their own records (no entries).
const NAV_GROUPS: { label: string; items: NavItem[] }[] = [
  {
    label: "Contabilidad",
    items: [
      { href: "/entries", label: "Asientos", icon: BookOpen },
      { href: "/accounts", label: "Cuentas", icon: Wallet },
    ],
  },
  {
    label: "Análisis",
    items: [
      { href: "/expenses", label: "Gastos", icon: Receipt },
      { href: "/budget", label: "Presupuesto", icon: Target },
      { href: "/reports", label: "Reportes", icon: BarChart2 },
    ],
  },
  {
    label: "Herramientas",
    items: [
      { href: "/card-expenses", label: "Tarjeta de crédito", icon: CreditCard },
      { href: "/shared-expenses", label: "Gastos compartidos", icon: Users },
    ],
  },
];

function NavLink({
  item,
  pathname,
  onNavigate,
}: {
  item: NavItem;
  pathname: string;
  onNavigate: () => void;
}) {
  const Icon = item.icon;
  const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
  return (
    <SidebarMenuItem>
      <SidebarMenuButton asChild isActive={isActive}>
        <Link href={item.href} onClick={onNavigate}>
          <Icon />
          <span>{item.label}</span>
        </Link>
      </SidebarMenuButton>
    </SidebarMenuItem>
  );
}

export function AppSidebar({
  userName,
  mcpUrl,
}: {
  userName: string;
  mcpUrl: string;
}) {
  const [mcpOpen, setMcpOpen] = useState(false);
  const pathname = usePathname();
  const { isMobile, setOpenMobile } = useSidebar();
  const closeMobile = () => {
    if (isMobile) setOpenMobile(false);
  };

  return (
    <Sidebar>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <Link href="/home">
                <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                  <CircleDollarSign className="size-4" />
                </div>
                <div className="flex flex-col gap-0.5 leading-none">
                  <span className="font-semibold">Finance App</span>
                  <span className="text-xs text-muted-foreground">
                    Personal
                  </span>
                </div>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarMenu>
            <NavLink item={HOME_ITEM} pathname={pathname} onNavigate={closeMobile} />
          </SidebarMenu>
        </SidebarGroup>
        {NAV_GROUPS.map((group) => (
          <SidebarGroup key={group.label}>
            <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
            <SidebarMenu>
              {group.items.map((item) => (
                <NavLink
                  key={item.href}
                  item={item}
                  pathname={pathname}
                  onNavigate={closeMobile}
                />
              ))}
            </SidebarMenu>
          </SidebarGroup>
        ))}
      </SidebarContent>

      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <SidebarMenuButton size="lg">
                  <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-muted">
                    <User2 className="size-4" />
                  </div>
                  <div className="flex flex-col gap-0.5 leading-none overflow-hidden">
                    <span className="font-medium truncate">{userName}</span>
                  </div>
                  <ChevronsUpDown className="ml-auto size-4 shrink-0" />
                </SidebarMenuButton>
              </DropdownMenuTrigger>
              <DropdownMenuContent side="top" align="start">
                <DropdownMenuLabel>{userName}</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem onSelect={() => setMcpOpen(true)}>
                  <Plug />
                  Conectar MCP
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem
                  variant="destructive"
                  onSelect={async () => {
                    await logoutAction();
                  }}
                >
                  <LogOut />
                  Cerrar sesión
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </SidebarMenuItem>
        </SidebarMenu>
        <ConnectMcpSheet
          mcpUrl={mcpUrl}
          open={mcpOpen}
          onOpenChange={setMcpOpen}
        />
      </SidebarFooter>
    </Sidebar>
  );
}
