import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';

import {
  getCurrentUser,
  login,
  loginWithWindows,
  logout,
} from '../api/authApi';
import type { AuthUser, LoginRequest } from './types';

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  error: string | null;
  login: (request: LoginRequest) => Promise<void>;
  loginWindows: () => Promise<void>;
  logout: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
  clearError: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refreshUser = useCallback(async () => {
    setIsLoading(true);

    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
      setError(null);
    } catch (requestError) {
      setUser(null);
      setError(
        requestError instanceof Error
          ? requestError.message
          : 'Не удалось определить текущего пользователя.',
      );
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    void refreshUser();
  }, [refreshUser]);

  const handleLogin = useCallback(async (request: LoginRequest) => {
    setIsLoading(true);
    setError(null);

    try {
      const loggedInUser = await login(request);
      setUser(loggedInUser);
    } catch (requestError) {
      setUser(null);
      const message =
        requestError instanceof Error
          ? requestError.message
          : 'Не удалось выполнить вход.';
      setError(message);
      throw requestError;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const handleWindowsLogin = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const loggedInUser = await loginWithWindows();
      setUser(loggedInUser);
    } catch (requestError) {
      setUser(null);
      const message =
        requestError instanceof Error
          ? requestError.message
          : 'Не удалось выполнить Windows-вход.';
      setError(message);
      throw requestError;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const handleLogout = useCallback(async () => {
    setIsLoading(true);

    try {
      await logout();
      setUser(null);
      setError(null);
    } catch (requestError) {
      const message =
        requestError instanceof Error
          ? requestError.message
          : 'Не удалось завершить сеанс.';
      setError(message);
      throw requestError;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const clearError = useCallback(() => setError(null), []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isLoading,
      isAuthenticated: user !== null,
      error,
      login: handleLogin,
      loginWindows: handleWindowsLogin,
      logout: handleLogout,
      hasPermission: (permission: string) =>
        user?.permissions.includes(permission) ?? false,
      hasRole: (role: string) => user?.roles.includes(role) ?? false,
      clearError,
    }),
    [
      clearError,
      error,
      handleLogin,
      handleLogout,
      handleWindowsLogin,
      isLoading,
      user,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider.');
  }

  return context;
}
