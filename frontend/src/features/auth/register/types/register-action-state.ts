import { z } from "zod";
import { ActionState } from "@/src/shared/types/action-state";
import { RegisterSchema } from "./register-schema";

type RegisterErrors = Partial<
  Record<keyof z.infer<typeof RegisterSchema>, string>
>;

export type RegisterActionState = ActionState<RegisterErrors>;
