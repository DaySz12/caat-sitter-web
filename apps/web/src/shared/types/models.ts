export type UserRole = 'owner' | 'sitter' | 'admin';
export type AccountStatus = 'pending' | 'approved' | 'rejected' | 'suspended';
export type BookingStatus = 'pending' | 'confirmed' | 'in_progress' | 'completed' | 'cancelled' | 'rejected';
export type PaymentStatus = 'unpaid' | 'pending_verification' | 'paid' | 'rejected' | 'refunded';

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
  paymentSlipUrl: string | null;
  paymentSlipUploadedAt: string | null;
  paymentRejectionReason: string | null;
  specialInstructions: string | null;
  cancellationReason: string | null;
  createdAt: string;
  owner: UserDto | null;
  sitter: UserDto | null;
  cats: CatDto[];
  review?: ReviewDto | null;
}

export interface UserBriefDto {
  id: string;
  name: string;
  avatarUrl: string | null;
  role: UserRole;
}

export interface ReviewDto {
  id: string;
  bookingId: string;
  sitterId: string;
  rating: number;
  comment: string | null;
  photos: string[];
  createdAt: string;
  reviewer: UserBriefDto | null;
}

export interface NotificationDto {
  id: string;
  title: string;
  body: string;
  type: string;
  bookingId: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface UserSummaryDto {
  unreadNotifications: number;
  unreadMessages: number;
  pendingRequests: number;
  activeBookings: number;
  completedBookings: number;
  totalEarnings: number;
  monthEarnings: number;
  totalSpent: number;
}

export interface ChatRoomDto {
  id: string;
  other: UserBriefDto;
  bookingId: string | null;
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
}

export interface MessageDto {
  id: string;
  roomId: string;
  senderId: string;
  content: string | null;
  imageUrl: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface AttendanceDto {
  id: string;
  checkInTime: string;
  checkOutTime: string | null;
  locationLat: number | null;
  locationLng: number | null;
  checkinPhotoUrl: string | null;
  checkoutPhotoUrl: string | null;
  note: string | null;
}

export interface ChecklistItemDto {
  id: string;
  taskName: string;
  isCompleted: boolean;
  photoUrl: string | null;
  completedAt: string | null;
  note: string | null;
}

export interface ChecklistDto {
  id: string;
  date: string;
  title: string;
  items: ChecklistItemDto[];
}

export interface BookingCareDto {
  attendance: AttendanceDto[];
  checklists: ChecklistDto[];
}

export interface BannerDto {
  id: string;
  type: 'banner' | 'popup';
  title: string;
  imageUrl: string;
  linkUrl: string | null;
  isActive: boolean;
  displayOrder: number;
  createdAt: string;
}

