import { cookies } from "next/headers";
import { apiBaseUrl } from "./envs";

export async function apiFetch<T>(path: string, options?: RequestInit) {
  const res = await fetch(`/api${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
  });

  if (!res.ok) throw new Error("Error en la petición");

  return res.json();
}

interface FetchOptions extends RequestInit {
  auth?: boolean; // Si necesita token o no
}

export async function serverFetch<T>(
  path: string,
  options: FetchOptions = {},
): Promise<T> {
  const { auth = true, headers: customHeaders, ...rest } = options;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(customHeaders as Record<string, string>),
  };

  // Si la ruta necesita auth, sacás el token de la cookie
  if (auth) {
    const cookieStore = await cookies();
    const token = cookieStore.get("token")?.value;

    if (!token) throw new Error("No autorizado");

    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${apiBaseUrl}${path}`, {
    ...rest,
    headers,
  });

  return res.json() as Promise<T>;
}
