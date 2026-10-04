export type UserRole = 'owner' | 'sitter' | 'admin';
export type AccountStatus = 'pending' | 'approved' | 'rejected' | 'suspended';
export type BookingStatus = 'pending' | 'confirmed' | 'in_progress' | 'completed' | 'cancelled' | 'rejected';
export type PaymentStatus = 'unpaid' | 'paid' | 'refunded';

export interface UserDto {
  id: string;
  email: string;
  name: string;
  phone: string | null;
  avatarUrl: string | null;
  role: UserRole;
  status: AccountStatus;
  walletBalance: number;
  createdAt: string;
}

export interface CatDto {
  id: string;
  ownerId: string;
  name: string;
  breed: string | null;
  ageYears: number;
  ageMonths: number;
  weight: number | null;
  gender: 'male' | 'female';
  isNeutered: boolean;
  photoUrl: string | null;
  dietaryHabits: string | null;
  medicalNotes: string | null;
  vaccines: string[];
  createdAt: string;
}

export interface SitterDto {
  id: string;
  bio: string | null;
  pricePerDay: number;
  address: string | null;
  latitude: number | null;
  longitude: number | null;
  experienceYears: number;
  maxCatsPerDay: number;
  servicePhotos: string[];
  isVerified: boolean;
  isAvailable: boolean;
  ratingAvg: number;
  reviewCount: number;
  socialLine: string | null;
  socialFacebook: string | null;
  socialInstagram: string | null;
  user: UserDto | null;
  distanceKm: number | null;
  matchScore: number | null;
  specialSkills: string[];
}

export interface BookingDto {
  id: string;
  bookingCode: string;
  ownerId: string;
  sitterId: string;
  startDate: string;
  endDate: string;
  daysCount: number;
  basePrice: number;
  serviceFee: number;
  totalPrice: number;
  status: BookingStatus;
  paymentStatus: PaymentStatus;
  specialInstructions: string | null;
  cancellationReason: string | null;
  createdAt: string;
  owner: UserDto | null;
  sitter: UserDto | null;
  cats: CatDto[];
}
