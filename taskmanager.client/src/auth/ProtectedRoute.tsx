import type { PropsWithChildren, ReactNode } from 'react';

import { useAuth } from './AuthContext';

interface ProtectedRouteProps extends PropsWithChildren {
  fallback: ReactNode;
}

export function ProtectedRoute({ children, fallback }: ProtectedRouteProps) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="auth-loading-screen">
        <div className="loading-spinner" aria-hidden="true" />
        <span>Проверяем сеанс…</span>
      </div>
    );
  }

  return isAuthenticated ? children : fallback;
}
