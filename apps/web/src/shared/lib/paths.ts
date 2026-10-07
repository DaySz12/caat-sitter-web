import type { UserRole } from '@/shared/types/models';

/** Root of the signed-in user's section of the app. */
export function roleBase(role: UserRole | undefined): '/owner' | '/sitter' {
  return role === 'sitter' ? '/sitter' : '/owner';
}

export function bookingDetailPath(role: UserRole | undefined, bookingId: string): string {
  return role === 'sitter' ? `/sitter/jobs/${bookingId}` : `/owner/bookings/${bookingId}`;
}
