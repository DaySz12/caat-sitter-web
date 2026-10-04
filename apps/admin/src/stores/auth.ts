import { defineStore } from 'pinia';
import { ref } from 'vue';
import { api, getToken, setToken, ApiError } from '../api/client';

export interface UserProfile {
  id: string;
  email: string;
  name: string;
  role: 'owner' | 'sitter' | 'admin';
  status: 'pending' | 'approved' | 'rejected' | 'suspended';
  avatarUrl?: string;
}

export const useAuthStore = defineStore('adminAuth', () => {
  const profile = ref<UserProfile | null>(null);
  const isLoading = ref(true);
  const error = ref<string | null>(null);

  async function checkSession(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;
    const token = getToken();
    if (!token) {
      profile.value = null;
      isLoading.value = false;
      return false;
    }

    try {
      const me = await api.get<UserProfile>('/api/auth/me');
      if (me.role !== 'admin') {
        error.value = 'คุณไม่มีสิทธิ์เข้าถึงส่วนผู้ดูแลระบบ (Admin Only)';
        setToken(null);
        profile.value = null;
        return false;
      }
      profile.value = me;
      return true;
    } catch {
      setToken(null);
      profile.value = null;
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function login(email: string, password: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;
    try {
      const res = await api.post<{ token: string; user: UserProfile }>('/api/auth/login', { email, password });

      if (res.user.role !== 'admin') {
        error.value = 'บัญชีนี้ไม่ใช่ผู้ดูแลระบบ (Admin Only)';
        return false;
      }

      setToken(res.token);
      profile.value = res.user;
      return true;
    } catch (e) {
      error.value = e instanceof ApiError ? e.message : 'เกิดข้อผิดพลาดในการเข้าสู่ระบบ';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function logout() {
    setToken(null);
    profile.value = null;
  }

  return { profile, isLoading, error, checkSession, login, logout };
});
