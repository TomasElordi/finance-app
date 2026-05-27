"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { LoginSchema } from "@/src/features/auth/login/types/login-schema";
import { redirect, RedirectType } from "next/navigation";
import { PAGES } from "@/src/shared/lib/pages";
import { LoginActionState } from "@/src/features/auth/login/types/login-action-state";
import { AuthResponse } from "@/src/features/auth/shared/types/auth-response";
import { session } from "@/src/shared/lib/session";

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
      message: "Invalid credencials.",
      errors: {
        email: fieldErrors.email?.[0],
        password: fieldErrors.password?.[0],
      },
    };
  }
  try {
    const response = await serverFetch<ApiResponse<AuthResponse>>(
      "/auth/login",
      {
        method: "POST",
        auth: false, // No necesita token
        body: JSON.stringify(parsed.data),
      },
    );

    if (!response.success) {
      return { status: "error", message: response.message, errors: {} };
    }
    await session.setAccessToken(response.data.accessToken);
    await session.setUserName(response.data.user.name);
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
  redirect(PAGES.HOME, RedirectType.replace);
}
