import { defineStore } from 'pinia';
import { ref } from 'vue';
import { authApi } from './auth.api';
import { getToken, setToken } from '@/shared/lib/api';
import type { UserDto } from '@/shared/types/models';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserDto | null>(null);
  const isLoading = ref(true);
  const error = ref<string | null>(null);

  async function checkSession(): Promise<boolean> {
    isLoading.value = true;
    error.value = null;
    const token = getToken();
    if (!token) {
      user.value = null;
      isLoading.value = false;
      return false;
    }
    try {
      user.value = await authApi.me();
      return true;
    } catch {
      setToken(null);
      user.value = null;
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function login(email: string, password: string): Promise<boolean> {
    isLoading.value = true;
    error.value = null;
    try {
      const res = await authApi.login(email, password);
      setToken(res.token);
      user.value = res.user;
      return true;
    } catch (e: any) {
      error.value = e.message || 'เข้าสู่ระบบไม่สำเร็จ';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function register(params: { email: string; password: string; name: string; phone?: string; role: 'owner' | 'sitter' }): Promise<boolean> {
    isLoading.value = true;
    error.value = null;
    try {
      const res = await authApi.register(params);
      setToken(res.token);
      user.value = res.user;
      return true;
    } catch (e: any) {
      error.value = e.message || 'สมัครสมาชิกไม่สำเร็จ';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function ssoLogin(payload: import('./auth.api').SsoLoginPayload): Promise<boolean> {
    isLoading.value = true;
    error.value = null;
    try {
      const res = await authApi.sso(payload);
      setToken(res.token);
      user.value = res.user;
      return true;
    } catch (e: any) {
      error.value = e.message || 'เข้าสู่ระบบด้วย SSO ไม่สำเร็จ';
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function logout() {
    setToken(null);
    user.value = null;
  }

  return { user, isLoading, error, checkSession, login, ssoLogin, register, logout };
});
