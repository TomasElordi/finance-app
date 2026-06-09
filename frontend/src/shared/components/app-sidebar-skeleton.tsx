import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuItem,
} from "./ui/sidebar";
import { Skeleton } from "./ui/skeleton";

export default function AppSidebarSkeleton() {
  return (
    <Sidebar>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <div className="flex items-center gap-2 px-2 py-1.5">
              <Skeleton className="size-8 rounded-lg shrink-0" />
              <div className="flex flex-col gap-1">
                <Skeleton className="h-3.5 w-24" />
                <Skeleton className="h-3 w-14" />
              </div>
            </div>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Navegación</SidebarGroupLabel>
          <SidebarMenu>
            {Array.from({ length: 4 }).map((_, i) => (
              <SidebarMenuItem key={i}>
                <div className="flex items-center gap-2 px-2 py-1.5">
                  <Skeleton className="size-4 rounded-sm shrink-0" />
                  <Skeleton className="h-3.5 w-20" />
                </div>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter>
        <SidebarMenu>
          <SidebarMenuItem>
            <div className="flex items-center gap-2 px-2 py-1.5">
              <Skeleton className="size-8 rounded-lg shrink-0" />
              <Skeleton className="h-3.5 w-28" />
              <Skeleton className="ml-auto size-4 rounded-sm shrink-0" />
            </div>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarFooter>
    </Sidebar>
  );
}
