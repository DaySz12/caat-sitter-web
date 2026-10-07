import { api } from '@/shared/lib/api';
import type { ReviewDto } from '@/shared/types/models';

export const reviewsApi = {
  create: (payload: { bookingId: string; rating: number; comment?: string; photos?: string[] }) => api.post<ReviewDto>('/api/reviews', payload),
  forSitter: (sitterId: string) => api.get<ReviewDto[]>(`/api/sitters/${sitterId}/reviews`),
};
