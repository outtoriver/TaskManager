export interface AuthUser {
  id: number;
  login: string;
  displayName: string;
  email: string | null;
  departmentName: string | null;
  positionName: string | null;
  managerName: string | null;
  isActive: boolean;
  authenticationType: string;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  login: string;
  password: string;
}

export interface ApiError {
  message?: string;
}
