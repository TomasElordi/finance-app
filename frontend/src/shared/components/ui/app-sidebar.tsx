"use client";

import Link from "next/link";
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
} from "./sidebar";
import {
  BarChart2,
  BookOpen,
  ChevronsUpDown,
  CircleDollarSign,
  LogOut,
  Target,
  User2,
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

export function AppSidebar({ userName }: { userName: string }) {
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
          <SidebarGroupLabel>Navegación</SidebarGroupLabel>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild>
                <Link href="/accounts">
                  <Wallet />
                  <span>Cuentas</span>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
            <SidebarMenuItem>
              <SidebarMenuButton asChild>
                <Link href="/entries">
                  <BookOpen />
                  <span>Asientos</span>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
            <SidebarMenuItem>
              <SidebarMenuButton asChild>
                <Link href="/reports">
                  <BarChart2 />
                  <span>Reportes</span>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
            <SidebarMenuItem>
              <SidebarMenuButton asChild>
                <Link href="/budget">
                  <Target />
                  <span>Presupuesto</span>
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroup>
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
      </SidebarFooter>
    </Sidebar>
  );
}
