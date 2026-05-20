import { z } from "zod";
import { ActionState } from "@/src/shared/types/action-state";
import { LoginSchema } from "./login-schema";

type LoginErrors = Partial<Record<keyof z.infer<typeof LoginSchema>, string>>;

export type LoginActionState = ActionState<LoginErrors>;
