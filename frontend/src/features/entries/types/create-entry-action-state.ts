import { ActionState } from "@/src/shared/types/action-state";

type CreateEntryErrors = Partial<
  Record<"title" | "description" | "date" | "entryLines", string>
>;

export type CreateEntryActionState = ActionState<CreateEntryErrors>;
