"use client";

import { useRouter, useSearchParams } from "next/navigation";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/src/shared/components/ui/select";
import { SharedGroupSummary } from "../types/shared";

export default function GroupSwitcher({ groups, groupId }: { groups: SharedGroupSummary[]; groupId: string }) {
  const router = useRouter();
  const searchParams = useSearchParams();

  if (groups.length < 2) return null;

  return (
    <Select
      value={groupId}
      onValueChange={(id) => {
        const params = new URLSearchParams(searchParams.toString());
        params.set("group", id);
        params.delete("join");
        router.push(`?${params.toString()}`);
      }}
    >
      <SelectTrigger className="w-48">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {groups.map((g) => (
          <SelectItem key={g.id} value={g.id}>
            {g.name}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
