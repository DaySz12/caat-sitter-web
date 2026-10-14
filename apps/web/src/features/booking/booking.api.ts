import { api } from '@/shared/lib/api';
import type { BookingDto, BookingStatus } from '@/shared/types/models';

export interface CreateBookingPayload {
  sitterId: string;
  catIds: string[];
  startDate: string; // yyyy-MM-dd
  endDate: string;
  specialInstructions?: string;
}

export const bookingApi = {
  mine: (status?: BookingStatus | 'all') =>
    api.get<BookingDto[]>(`/api/bookings/mine${status ? `?status=${status}` : ''}`),
  getById: (id: string) => api.get<BookingDto>(`/api/bookings/${id}`),
  create: (payload: CreateBookingPayload) => api.post<BookingDto>('/api/bookings', payload),
  updateStatus: (id: string, status: BookingStatus, cancellationReason?: string) =>
    api.patch<BookingDto>(`/api/bookings/${id}/status`, { status, cancellationReason }),
  uploadPaymentSlip: (id: string, slipUrl: string) =>
    api.post<BookingDto>(`/api/bookings/${id}/payment-slip`, { slipUrl }),
};
