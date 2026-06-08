"use client";

import { Menu } from "lucide-react";
import { useSidebar } from "./ui/sidebar";

export default function TriggerSidebar() {
  const { toggleSidebar } = useSidebar();
  return (
    <button
      onClick={toggleSidebar}
      className="p-2 bg-sidebar rounded-br-lg rounded-tr-lg md:hidden block"
    >
      <Menu />
    </button>
  );
}
