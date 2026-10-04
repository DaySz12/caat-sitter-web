import { api } from '@/shared/lib/api';
import type { UserDto, UserRole } from '@/shared/types/models';

export interface AuthResponse {
  token: string;
  user: UserDto;
}

export const authApi = {
  login: (email: string, password: string) =>
    api.post<AuthResponse>('/api/auth/login', { email, password }),

  register: (params: { email: string; password: string; name: string; phone?: string; role: UserRole }) =>
    api.post<AuthResponse>('/api/auth/register', params),

  me: () => api.get<UserDto>('/api/auth/me'),

  updateProfile: (params: { name?: string; phone?: string; avatarUrl?: string }) =>
    api.put<UserDto>('/api/users/me', params),
};
