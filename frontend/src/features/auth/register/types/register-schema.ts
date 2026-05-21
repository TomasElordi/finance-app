import z from "zod";

export const RegisterSchema = z.object({
  name: z.string().min(3, "The name must have a minimum of 3 characters."),
  email: z.string().email("Invalid email."),
  password: z
    .string()
    .min(6, "The password must have a minimum of 6 characters."),
  confirm_password: z
    .string()
    .min(6, "The confirm password must have a minimum of 6 characters."),
});
