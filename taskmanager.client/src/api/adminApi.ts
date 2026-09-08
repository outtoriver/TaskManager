export interface UserListItem {
  id: number;
  login: string;
  displayName: string;
  email: string | null;
  isActive: boolean;
  departmentId: number | null;
  departmentName: string | null;
  positionId: number | null;
  positionName: string | null;
  managerId: number | null;
  managerName: string | null;
}

export interface UserRole {
  id: number;
  name: string;
  description: string | null;
  isSystemRole: boolean;
}

export interface UserDetails extends UserListItem {
  phone: string | null;
  createdAt: string;
  lastLoginAt: string | null;
  roles: UserRole[];
  subordinates: UserListItem[];
}

export interface Department {
  id: number;
  name: string;
  description: string | null;
  isActive: boolean;
  userCount: number;
}

export interface DepartmentDetails extends Department {
  users: UserListItem[];
}

export interface Position {
  id: number;
  name: string;
  isManagerPosition: boolean;
  isActive: boolean;
  userCount: number;
}

export interface PositionDetails extends Position {
  users: UserListItem[];
}

export interface Role {
  id: number;
  name: string;
  description: string | null;
  isSystemRole: boolean;
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    credentials: 'include',
    headers: {
      ...(init?.body ? { 'Content-Type': 'application/json' } : {}),
      ...(init?.headers ?? {}),
    },
    ...init,
  });

  if (!response.ok) {
    let message = `Ошибка запроса (${response.status}).`;
    try {
      const payload = (await response.json()) as { message?: string };
      if (payload.message) message = payload.message;
    } catch {
      // Non-JSON response.
    }
    throw new Error(message);
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

export const adminApi = {
  getUsers: () => request<UserListItem[]>('/api/users'),
  getUser: (id: number) => request<UserDetails>(`/api/users/${id}`),
  createUser: (payload: {
    login: string;
    displayName: string;
    email?: string | null;
    phone?: string | null;
    departmentId?: number | null;
    positionId?: number | null;
    managerId?: number | null;
    isActive: boolean;
  }) => request<UserDetails>('/api/users', { method: 'POST', body: JSON.stringify(payload) }),
  updateUser: (id: number, payload: {
    displayName: string;
    email?: string | null;
    phone?: string | null;
    departmentId?: number | null;
    positionId?: number | null;
    managerId?: number | null;
    isActive: boolean;
  }) => request<UserDetails>(`/api/users/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deleteUser: (id: number) => request<void>(`/api/users/${id}`, { method: 'DELETE' }),
  getUserRoles: (id: number) => request<UserRole[]>(`/api/users/${id}/roles`),
  setUserRoles: (id: number, roleIds: number[]) => request<void>(`/api/users/${id}/roles`, {
    method: 'PUT', body: JSON.stringify({ roleIds }),
  }),

  getDepartments: () => request<Department[]>('/api/departments'),
  getDepartment: (id: number) => request<DepartmentDetails>(`/api/departments/${id}`),
  createDepartment: (payload: { name: string; description?: string | null }) =>
    request<DepartmentDetails>('/api/departments', { method: 'POST', body: JSON.stringify(payload) }),
  updateDepartment: (id: number, payload: { name: string; description?: string | null; isActive: boolean }) =>
    request<DepartmentDetails>(`/api/departments/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deleteDepartment: (id: number) => request<void>(`/api/departments/${id}`, { method: 'DELETE' }),

  getPositions: () => request<Position[]>('/api/positions'),
  getPosition: (id: number) => request<PositionDetails>(`/api/positions/${id}`),
  createPosition: (payload: { name: string; isManagerPosition: boolean }) =>
    request<PositionDetails>('/api/positions', { method: 'POST', body: JSON.stringify(payload) }),
  updatePosition: (id: number, payload: { name: string; isManagerPosition: boolean; isActive: boolean }) =>
    request<PositionDetails>(`/api/positions/${id}`, { method: 'PUT', body: JSON.stringify(payload) }),
  deletePosition: (id: number) => request<void>(`/api/positions/${id}`, { method: 'DELETE' }),

  getRoles: () => request<Role[]>('/api/roles'),
};
