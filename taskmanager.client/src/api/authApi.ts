import type { AuthUser, LoginRequest } from '../auth/types';

async function parseError(response: Response): Promise<string> {
  try {
    const payload = (await response.json()) as { message?: string };
    if (payload.message) return payload.message;
  } catch {
    // Ignore invalid/non-JSON error responses.
  }

  return `Ошибка запроса (${response.status}).`;
}

export async function login(request: LoginRequest): Promise<AuthUser> {
  const response = await fetch('/api/auth/login', {
    method: 'POST',
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return (await response.json()) as AuthUser;
}

export async function loginWithWindows(): Promise<AuthUser> {
  const response = await fetch('/api/auth/windows', {
    method: 'GET',
    credentials: 'include',
  });

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return (await response.json()) as AuthUser;
}

export async function getCurrentUser(): Promise<AuthUser | null> {
  const response = await fetch('/api/auth/me', {
    method: 'GET',
    credentials: 'include',
  });

  if (response.status === 401) {
    return null;
  }

  if (!response.ok) {
    throw new Error(await parseError(response));
  }

  return (await response.json()) as AuthUser;
}

export async function logout(): Promise<void> {
  const response = await fetch('/api/auth/logout', {
    method: 'POST',
    credentials: 'include',
  });

  if (!response.ok && response.status !== 204) {
    throw new Error(await parseError(response));
  }
}
