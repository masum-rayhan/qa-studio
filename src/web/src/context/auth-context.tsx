"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import { authService, type AuthResponse, type UserProfile } from "@/services/auth";
import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, name: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
  });
  const router = useRouter();
  const queryClient = useQueryClient();
  const initRef = useRef(false);

  const storeTokens = (res: AuthResponse) => {
    localStorage.setItem("accessToken", res.accessToken);
    localStorage.setItem("refreshToken", res.refreshToken);
  };

  const logout = useCallback(() => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    queryClient.clear();
    setState({ user: null, isAuthenticated: false, isLoading: false });
    router.push("/login");
  }, [queryClient, router]);

  // Hydrate auth state from token on mount
  useEffect(() => {
    if (initRef.current) return;
    initRef.current = true;

    const token = localStorage.getItem("accessToken");
    if (!token) {
      return;
    }

    authService
      .getProfile()
      .then((user) => setState({ user, isAuthenticated: true, isLoading: false }))
      .catch(() => logout());
  }, [logout]);

  const login = async (email: string, password: string) => {
    const res = await authService.login({ email, password });
    storeTokens(res);
    setState({ user: res.user, isAuthenticated: true, isLoading: false });
    router.push("/dashboard");
  };

  const register = async (email: string, password: string, name: string) => {
    const res = await authService.register({ email, password, name });
    storeTokens(res);
    setState({ user: res.user, isAuthenticated: true, isLoading: false });
    router.push("/dashboard");
  };

  return (
    <AuthContext.Provider value={{ ...state, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
