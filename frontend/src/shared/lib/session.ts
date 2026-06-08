import { cookies } from "next/headers";
import { accessTokenMaxAge } from "./envs";
import { decodeJwt } from "jose";

const COOKIE_KEYS = {
  ACCESS_TOKEN: "accessToken",
  REFRESH_TOKEN: "refreshToken",
  USER_NAME: "userName",
};

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: process.env.NODE_ENV === "production",
  sameSite: "lax" as const,
};

export const session = {
  async setAccessToken(accessToken: string) {
    const store = await cookies();
    store.set(COOKIE_KEYS.ACCESS_TOKEN, accessToken, {
      ...COOKIE_OPTIONS,
      maxAge: accessTokenMaxAge,
    });
  },
  async setRefreshToken(refreshToken: string) {
    const store = await cookies();
    store.set(COOKIE_KEYS.REFRESH_TOKEN, refreshToken, {
      ...COOKIE_OPTIONS,
      maxAge: accessTokenMaxAge * 30,
    });
  },
  async getAccessToken() {
    const store = await cookies();
    return store.get(COOKIE_KEYS.ACCESS_TOKEN)?.value ?? null;
  },
  async getRefreshToken() {
    const store = await cookies();
    return store.get(COOKIE_KEYS.REFRESH_TOKEN)?.value ?? null;
  },

  async setUserName(name: string) {
    const store = await cookies();
    store.set(COOKIE_KEYS.USER_NAME, name, {
      ...COOKIE_OPTIONS,
      maxAge: accessTokenMaxAge,
    });
  },
  async getUserName() {
    const store = await cookies();
    return store.get(COOKIE_KEYS.USER_NAME)?.value ?? null;
  },

  async clear() {
    const store = await cookies();
    store.delete(COOKIE_KEYS.ACCESS_TOKEN);
    store.delete(COOKIE_KEYS.REFRESH_TOKEN);
    store.delete(COOKIE_KEYS.USER_NAME);
  },

  async isAuthenticated(): Promise<boolean> {
    const token = await this.getAccessToken();
    if (!token) return false;

    try {
      const { exp } = decodeJwt(token);
      if (!exp) return false;
      return exp * 1000 > Date.now(); // exp está en segundos, Date.now() en ms
    } catch {
      return false; // token malformado
    }
  },
};
