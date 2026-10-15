import { api } from '@/shared/lib/api';
import type { SitterDto } from '@/shared/types/models';

export interface SitterSearchParams {
  search?: string;
  maxPrice?: number;
  minRating?: number;
  userLat?: number;
  userLng?: number;
  sortBy?: 'smart' | 'distance' | 'rating' | 'price_asc';
  startDate?: string; // yyyy-MM-dd
  endDate?: string;
  requiredSkills?: string[];
}

function buildQuery(params: SitterSearchParams): string {
  const usp = new URLSearchParams();
  if (params.search) usp.set('search', params.search);
  if (params.maxPrice) usp.set('maxPrice', String(params.maxPrice));
  if (params.minRating) usp.set('minRating', String(params.minRating));
  if (params.userLat !== undefined) usp.set('userLat', String(params.userLat));
  if (params.userLng !== undefined) usp.set('userLng', String(params.userLng));
  if (params.sortBy) usp.set('sortBy', params.sortBy);
  if (params.startDate) usp.set('startDate', params.startDate);
  if (params.endDate) usp.set('endDate', params.endDate);
  for (const skill of params.requiredSkills ?? []) usp.append('requiredSkills', skill);
  return usp.toString();
}

export interface UpdateSitterProfilePayload {
  bio?: string;
  pricePerDay?: number;
  address?: string;
  latitude?: number;
  longitude?: number;
  experienceYears?: number;
  maxCatsPerDay?: number;
  servicePhotos?: string[];
  isAvailable?: boolean;
  socialLine?: string;
  socialFacebook?: string;
  socialInstagram?: string;
}

export interface SitterVerificationStatus {
  isVerified: boolean;
  status: 'pending' | 'approved' | 'rejected' | 'not_submitted';
  idCardUrl: string | null;
  rejectionReason: string | null;
  submittedAt: string | null;
}

export const sittersApi = {
  search: (params: SitterSearchParams = {}) => api.get<SitterDto[]>(`/api/sitters?${buildQuery(params)}`),
  getById: (id: string) => api.get<SitterDto>(`/api/sitters/${id}`),
  updateMine: (payload: UpdateSitterProfilePayload) => api.put<SitterDto>('/api/sitters/me', payload),
  getVerificationStatus: () => api.get<SitterVerificationStatus>('/api/sitters/me/verification'),
  submitVerification: (idCardUrl: string) =>
    api.post<SitterVerificationStatus>('/api/sitters/me/verification', { idCardUrl }),
};
