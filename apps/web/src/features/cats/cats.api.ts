import { api } from '@/shared/lib/api';
import type { CatDto } from '@/shared/types/models';

export interface CatUpsertPayload {
  name: string;
  breed?: string | null;
  ageYears: number;
  ageMonths: number;
  weight?: number | null;
  gender: 'male' | 'female';
  isNeutered: boolean;
  photoUrl?: string | null;
  dietaryHabits?: string | null;
  medicalNotes?: string | null;
  vaccines?: string[];
}

export const catsApi = {
  mine: () => api.get<CatDto[]>('/api/cats/mine'),
  getById: (id: string) => api.get<CatDto>(`/api/cats/${id}`),
  create: (payload: CatUpsertPayload) => api.post<CatDto>('/api/cats', payload),
  update: (id: string, payload: CatUpsertPayload) => api.put<CatDto>(`/api/cats/${id}`, payload),
  remove: (id: string) => api.delete<void>(`/api/cats/${id}`),
};
