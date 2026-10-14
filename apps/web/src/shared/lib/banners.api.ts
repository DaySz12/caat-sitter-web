import { api } from '@/shared/lib/api';
import type { BannerDto } from '@/shared/types/models';

export const bannersApi = {
  list: (type?: 'banner' | 'popup') =>
    api.get<BannerDto[]>(`/api/banners${type ? `?type=${type}` : ''}`),
};
