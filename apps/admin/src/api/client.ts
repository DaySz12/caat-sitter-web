// Thin fetch wrapper for the ASP.NET Core API — replaces the old @supabase/supabase-js client.
// Response shapes for admin endpoints are kept snake_case server-side to match what these
// views were already written against.

export const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5254';

const TOKEN_KEY = 'catsitter_admin_token';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

export function resolveImageUrl(url?: string | null): string {
  if (!url) return '';
  if (url.includes('.trycloudflare.com/uploads/')) {
    url = url.replace(/https?:\/\/[^/]+\/uploads\//, `${API_URL}/uploads/`);
  }
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url.startsWith('/') ? '' : '/'}${url}`;
}

export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken();
  const headers = new Headers(options.headers);
  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json');
  }
  if (token) headers.set('Authorization', `Bearer ${token}`);

  const res = await fetch(`${API_URL}${path}`, { ...options, headers });

  if (res.status === 204) return undefined as T;

  const isJson = res.headers.get('content-type')?.includes('application/json');
  const data = isJson ? await res.json().catch(() => null) : null;

  if (!res.ok) {
    const message = (data && (data.message || data.title)) || `เกิดข้อผิดพลาด (${res.status})`;
    throw new ApiError(message, res.status);
  }

  return data as T;
}

export const api = {
  get: <T>(path: string) => request<T>(path, { method: 'GET' }),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body !== undefined ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PUT', body: body !== undefined ? JSON.stringify(body) : undefined }),
  patch: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PATCH', body: body !== undefined ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string) => request<T>(path, { method: 'DELETE' }),
  upload: async (bucket: string, file: File): Promise<{ url: string }> => {
    const form = new FormData();
    form.append('file', file);
    const result = await request<{ url: string }>(`/api/uploads/${bucket}`, { method: 'POST', body: form });
    return { url: `${API_URL}${result.url}` };
  },
};
