import { api } from '@/shared/lib/api';
import type { BookingCareDto } from '@/shared/types/models';

// Every write returns the booking's whole care log.
export const careApi = {
  get: (bookingId: string) => api.get<BookingCareDto>(`/api/bookings/${bookingId}/care`),
  checkIn: (bookingId: string, payload: { lat?: number; lng?: number; photoUrl?: string; note?: string }) =>
    api.post<BookingCareDto>(`/api/bookings/${bookingId}/check-in`, payload),
  checkOut: (bookingId: string, payload: { photoUrl?: string; note?: string } = {}) =>
    api.post<BookingCareDto>(`/api/bookings/${bookingId}/check-out`, payload),
  updateItem: (itemId: string, payload: { isCompleted?: boolean; photoUrl?: string; note?: string }) =>
    api.patch<BookingCareDto>(`/api/checklist-items/${itemId}`, payload),
};
