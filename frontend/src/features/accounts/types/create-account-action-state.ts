import { ActionState } from "@/src/shared/types/action-state";

type CreateAccountErrors = Partial<Record<"name" | "nature", string>>;

export type CreateAccountActionState = ActionState<CreateAccountErrors>;
