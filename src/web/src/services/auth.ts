import api from "./api";

export interface RegisterDto {
  email: string;
  password: string;
  name: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  role: "Admin" | "QA";
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserProfile;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export const authService = {
  register: async (dto: RegisterDto): Promise<AuthResponse> => {
    const res = await api.post<{ data: AuthResponse }>("/auth/register", dto);
    return res.data.data;
  },

  login: async (dto: LoginDto): Promise<AuthResponse> => {
    const res = await api.post<{ data: AuthResponse }>("/auth/login", dto);
    return res.data.data;
  },

  refreshToken: async (refreshToken: string): Promise<TokenResponse> => {
    const res = await api.post<{ data: TokenResponse }>("/auth/refresh-token", {
      refreshToken,
    });
    return res.data.data;
  },

  getProfile: async (): Promise<UserProfile> => {
    const res = await api.get<{ data: UserProfile }>("/auth/me");
    return res.data.data;
  },
};
