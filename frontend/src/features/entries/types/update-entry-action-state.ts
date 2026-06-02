import { ActionState } from "@/src/shared/types/action-state";

type UpdateEntryErrors = Partial<
  Record<"title" | "description" | "date" | "entryLines", string>
>;

export type UpdateEntryActionState = ActionState<UpdateEntryErrors>;
