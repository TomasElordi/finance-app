import { z } from "zod";
import { NatureType } from "./account";

export const CreateAccountSchema = z.object({
  name: z.string().min(1, "El nombre es requerido"),
  nature: z.coerce
    .number()
    .refine((v) => Object.values(NatureType).includes(v), {
      message: "Tipo inválido",
    }),
});
