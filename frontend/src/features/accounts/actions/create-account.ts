"use server";

import { serverFetch } from "@/src/shared/lib/api";
import { ApiResponse } from "@/src/shared/types/api";
import { revalidateTag, refresh } from "next/cache";
import { CreateAccountSchema } from "@/src/features/accounts/types/create-account-schema";
import { CreateAccountActionState } from "@/src/features/accounts/types/create-account-action-state";
import { Account } from "@/src/features/accounts/types/account";

export async function createAccountAction(
  _prevState: CreateAccountActionState,
  formData: FormData,
): Promise<CreateAccountActionState> {
  const raw = {
    name: formData.get("name"),
    nature: formData.get("nature"),
  };

  const parsed = CreateAccountSchema.safeParse(raw);
  if (!parsed.success) {
    const { fieldErrors } = parsed.error.flatten();
    return {
      status: "error",
      message: "Datos inválidos.",
      errors: {
        name: fieldErrors.name?.[0],
        nature: fieldErrors.nature?.[0],
      },
    };
  }

  try {
    const response = await serverFetch<ApiResponse<Account>>("/account", {
      method: "POST",
      auth: true,
      body: JSON.stringify(parsed.data),
    });

    if (response && !response.success) {
      return { status: "error", message: response.message, errors: {} };
    }

    revalidateTag("accounts", {});
    refresh();
    return { status: "success" };
  } catch (error) {
    console.log("error", error);
    return { status: "error", message: "Error de conexión", errors: {} };
  }
}
