import { api } from '@/shared/lib/api';
import type { ChatRoomDto, MessageDto } from '@/shared/types/models';

export const chatApi = {
  rooms: () => api.get<ChatRoomDto[]>('/api/chat/rooms'),
  /** Opens the room with another user, or returns the existing one. */
  start: (userId: string, bookingId?: string) => api.post<ChatRoomDto>('/api/chat/rooms', { userId, bookingId }),
  messages: (roomId: string, after?: string) =>
    api.get<MessageDto[]>(`/api/chat/rooms/${roomId}/messages${after ? `?after=${encodeURIComponent(after)}` : ''}`),
  send: (roomId: string, payload: { content?: string; imageUrl?: string }) =>
    api.post<MessageDto>(`/api/chat/rooms/${roomId}/messages`, payload),
};
