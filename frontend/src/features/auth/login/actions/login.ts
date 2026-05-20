"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { LoginResponse } from "../types/login-response";
import { ApiResponse } from "@/src/shared/types/api";
import { cookies } from "next/headers";
import { LoginSchema } from "../types/login-schema";
import { redirect, RedirectType } from "next/navigation";
import { PAGES } from "@/src/shared/lib/pages";
import { LoginActionState } from "../types/login-action-state";

export async function loginAction(
  _prevState: LoginActionState,
  formData: FormData,
): Promise<LoginActionState> {
  // Extraés los datos del FormData
  const raw = {
    email: formData.get("email"),
    password: formData.get("password"),
  };

  // Validás con Zod
  const parsed = LoginSchema.safeParse(raw);
  if (!parsed.success) {
    const { fieldErrors } = parsed.error.flatten();
    return {
      status: "error",
      message: "Credenciales inválidas",
      errors: {
        email: fieldErrors.email?.[0],
        password: fieldErrors.password?.[0],
      },
    };
  }
  try {
    const response = await serverFetch<ApiResponse<LoginResponse>>(
      "/auth/login",
      {
        method: "POST",
        auth: false, // No necesita token
        body: JSON.stringify(parsed.data),
      },
    );

    console.log("response: ", response);

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
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
  redirect(PAGES.HOME, RedirectType.replace);
}
