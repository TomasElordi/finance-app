import { apiBaseUrl } from "./envs";
import { ApiResponse } from "../types/api";
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
    headers["Authorization"] = `Bearer ${token}`;
  }

  const res = await fetch(`${apiBaseUrl}${path}`, {
    ...rest,
    headers,
  });

  const text = await res.text();
  return (text ? JSON.parse(text) : null) as T;
}

// For cached data fetchers: throws instead of returning a fallback so that
// failures aren't cached as "no data" and reach the error boundary.
export async function fetchApiData<T>(path: string, token: string): Promise<T> {
  const response = await serverFetch<ApiResponse<T>>(path, {
    auth: false,
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!response?.success) {
    throw new Error(`GET ${path} failed: ${response?.message ?? "empty response"}`);
  }
  return response.data;
}
