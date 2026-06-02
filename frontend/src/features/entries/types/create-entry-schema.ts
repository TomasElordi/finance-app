import { z } from "zod";

const EntryLineSchema = z.object({
  accountId: z.string().min(1, "La cuenta es requerida"),
  amount: z.coerce.number().positive("El monto debe ser mayor a 0"),
  type: z.enum(["Credit", "Debit"], { message: "Tipo inválido" }),
});

export const CreateEntrySchema = z.object({
  title: z.string().min(1, "El título es requerido"),
  description: z.string().optional(),
  date: z.string().min(1, "La fecha es requerida"),
  entryLines: z
    .array(EntryLineSchema)
    .min(2, "Se necesitan al menos 2 líneas"),
});
