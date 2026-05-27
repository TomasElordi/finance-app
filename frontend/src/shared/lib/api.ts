import { apiBaseUrl } from "./envs";
import { session } from "./session";

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
  auth?: boolean;
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

  if (auth) {
    const token = await session.getAccessToken();

    if (!token) throw new Error("No autorizado");
    console.log("token:", token);
    headers["Authorization"] = `Bearer ${token}`;
  }
  console.log("path: ", path);

  const res = await fetch(`${apiBaseUrl}${path}`, {
    ...rest,
    headers,
  });

  const text = await res.text();
  console.log("RESPUESTA:", text);
  return (text ? JSON.parse(text) : null) as T;
}
