import z from "zod";

export const LoginSchema = z.object({
  email: z.string().email("Invalid email."),
  password: z
    .string()
    .min(6, "The password must have a minimum of 6 characters."),
});
