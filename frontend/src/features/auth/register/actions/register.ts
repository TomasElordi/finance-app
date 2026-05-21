"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { cookies } from "next/headers";
import { redirect, RedirectType } from "next/navigation";
import { PAGES } from "@/src/shared/lib/pages";
import { RegisterActionState } from "../types/register-action-state";
import { RegisterSchema } from "../types/register-schema";
import { AuthResponse } from "../../shared/types/auth-response";

export async function registerAction(
  _prevState: RegisterActionState,
  formData: FormData,
): Promise<RegisterActionState> {
  // Extraés los datos del FormData
  const raw = {
    name: formData.get("name"),
    email: formData.get("email"),
    password: formData.get("password"),
    confirm_password: formData.get("confirm_password"),
  };

  // Validás con Zod
  const parsed = RegisterSchema.safeParse(raw);
  if (!parsed.success) {
    const { fieldErrors } = parsed.error.flatten();
    return {
      status: "error",
      message: "Invalid credentials.",
      errors: {
        name: fieldErrors.name?.[0],
        email: fieldErrors.email?.[0],
        password: fieldErrors.password?.[0],
      },
    };
  }
  if (parsed.data.confirm_password !== parsed.data.password) {
    return {
      status: "error",
      message: "Password and confirm password must be equals.",
      errors: {},
    };
  }
  try {
    const response = await serverFetch<ApiResponse<AuthResponse>>(
      "/auth/register",
      {
        method: "POST",
        auth: false, // No necesita token
        body: JSON.stringify(parsed.data),
      },
    );
    if (!response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    // Guardás la cookie (esto corre en el servidor)
    const cookieStore = await cookies();
    cookieStore.set("token", response.data.accessToken, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      sameSite: "strict",
      maxAge: 60 * 60 * 24,
    });
    //TASK: Save USER!
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
  redirect(PAGES.HOME, RedirectType.replace);
}
