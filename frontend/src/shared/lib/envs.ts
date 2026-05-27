export const apiBaseUrl = process.env.API_URL;
export const accessTokenMaxAge: number =
  Number(process.env.ACCESS_TOKEN_MAX_AGE) ?? 60 * 60 * 24;
