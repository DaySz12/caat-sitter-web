import { api } from '@/shared/lib/api';
import type { PayoutDto } from '@/shared/types/models';

export interface CreatePayoutPayload {
  amount: number;
  bankName: string;
  bankAccountNumber: string;
  bankAccountName: string;
  promptPayNumber?: string;
}

export const payoutsApi = {
  mine: () => api.get<PayoutDto[]>('/api/payouts/mine'),
  request: (payload: CreatePayoutPayload) => api.post<PayoutDto>('/api/payouts', payload),
};
