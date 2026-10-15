import { api } from '@/shared/lib/api';
import type { UserDto, UserRole } from '@/shared/types/models';

export interface AuthResponse {
  token: string;
  user: UserDto;
}

export interface UpdateProfileParams {
  name?: string;
  phone?: string;
  avatarUrl?: string;
  address?: string;
  buildingNote?: string;
  latitude?: number;
  longitude?: number;
  lineId?: string;
  secondaryPhone?: string;
  emergencyContactName?: string;
}

export interface SsoLoginPayload {
  provider: 'line' | 'google';
  ssoId: string;
  email?: string;
  name: string;
  avatarUrl?: string;
  role?: UserRole;
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/api/auth/login', { email, password }),

  sso: (payload: SsoLoginPayload) =>
    api.post<AuthResponse>('/api/auth/sso', payload),

  register: (params: { email: string; password: string; name: string; phone?: string; role: UserRole }) =>
    api.post<AuthResponse>('/api/auth/register', params),

  me: () => api.get<UserDto>('/api/auth/me'),

  updateProfile: (params: UpdateProfileParams) =>
    api.put<UserDto>('/api/users/me', params),

  changePassword: (params: { currentPassword: string; newPassword: string }) =>
    api.post<{ message: string }>('/api/users/me/change-password', params),
};
