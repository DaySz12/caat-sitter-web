import { api } from '@/shared/lib/api';
import type { NotificationDto, UserSummaryDto } from '@/shared/types/models';

export const notificationsApi = {
  list: () => api.get<NotificationDto[]>('/api/notifications'),
  markRead: (id: string) => api.post<void>(`/api/notifications/${id}/read`),
  markAllRead: () => api.post<void>('/api/notifications/read-all'),
  summary: () => api.get<UserSummaryDto>('/api/users/me/summary'),
};
