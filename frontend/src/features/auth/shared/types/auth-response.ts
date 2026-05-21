import { User } from "@/src/shared/types/user";

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}
