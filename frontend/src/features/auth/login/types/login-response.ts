import { User } from "@/src/shared/types/user";

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}
