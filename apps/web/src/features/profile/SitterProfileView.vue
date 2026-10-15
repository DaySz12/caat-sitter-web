<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import {
  User,
  Loader2,
  CheckCircle2,
  AlertCircle,
  MapPin,
  Camera,
  ShieldCheck,
  Clock,
  Sparkles,
  Star,
  Briefcase,
  Calendar,
  Wallet,
  Lock,
  KeyRound,
  Phone,
  Trash2,
  Plus,
  ArrowRight,
  ExternalLink
} from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { sittersApi } from '@/features/sitters/sitters.api';
import { authApi } from '@/features/auth/auth.api';
import { bookingApi } from '@/features/booking/booking.api';
import { api, resolveImageUrl } from '@/shared/lib/api';
import { formatCurrency } from '@/shared/lib/format';
import PayoutModal from '@/features/payouts/PayoutModal.vue';

const auth = useAuthStore();

// Basic User Info
const name = ref('');
const phone = ref('');
const secondaryPhone = ref('');
const emergencyContactName = ref('');
const avatarUrl = ref<string | null>(null);

// Sitter Profile Info
const bio = ref('');
const pricePerDay = ref(300);
const address = ref('');
const latitude = ref<number | null>(null);
const longitude = ref<number | null>(null);
const experienceYears = ref(1);
const maxCatsPerDay = ref(3);
const isAvailable = ref(true);
const isVerified = ref(false);
const ratingAvg = ref(5.0);
const reviewCount = ref(0);
const socialLine = ref('');
const socialFacebook = ref('');
const socialInstagram = ref('');
const servicePhotos = ref<string[]>([]);

// Stats
const jobCount = ref(0);
const showPayoutModal = ref(false);

// Status flags
const isLoading = ref(true);
const isSaving = ref(false);
const isUploadingAvatar = ref(false);
const isUploadingPhoto = ref(false);
const isLocating = ref(false);
const savedMessage = ref('');
const errorMessage = ref('');

// Password state
const currentPassword = ref('');
const newPassword = ref('');
const confirmPassword = ref('');
const isChangingPassword = ref(false);
const passwordMessage = ref('');
const passwordError = ref('');

const resolvedAvatar = computed(() => resolveImageUrl(avatarUrl.value));

onMounted(async () => {
  if (!auth.user) return;
  name.value = auth.user.name ?? '';
  phone.value = auth.user.phone ?? '';
  secondaryPhone.value = auth.user.secondaryPhone ?? '';
  emergencyContactName.value = auth.user.emergencyContactName ?? '';
  avatarUrl.value = auth.user.avatarUrl ?? null;

  try {
    const [profile, bookings] = await Promise.all([
      sittersApi.getById(auth.user.id),
      bookingApi.mine('all').catch(() => []),
    ]);

    bio.value = profile.bio ?? '';
    pricePerDay.value = profile.pricePerDay;
    address.value = profile.address ?? '';
    latitude.value = profile.latitude;
    longitude.value = profile.longitude;
    experienceYears.value = profile.experienceYears;
    maxCatsPerDay.value = profile.maxCatsPerDay;
    isAvailable.value = profile.isAvailable;
    isVerified.value = profile.isVerified;
    ratingAvg.value = profile.ratingAvg;
    reviewCount.value = profile.reviewCount;
    socialLine.value = profile.socialLine ?? '';
    socialFacebook.value = profile.socialFacebook ?? '';
    socialInstagram.value = profile.socialInstagram ?? '';
    servicePhotos.value = profile.servicePhotos ?? [];
    jobCount.value = bookings.length;
  } catch (err) {
    console.error('Failed to load sitter profile', err);
  } finally {
    isLoading.value = false;
  }
});

function useMyLocation() {
  if (!navigator.geolocation) {
    errorMessage.value = 'เบราว์เซอร์ไม่รองรับการดึงพิกัด GPS';
    return;
  }
  isLocating.value = true;
  errorMessage.value = '';
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      latitude.value = pos.coords.latitude;
      longitude.value = pos.coords.longitude;
      isLocating.value = false;
    },
    () => {
      errorMessage.value = 'ไม่สามารถดึงตำแหน่งปัจจุบันได้ โปรดอนุญาตสิทธิ์เข้าถึงตำแหน่ง';
      isLocating.value = false;
    }
  );
}

async function handleAvatarChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  isUploadingAvatar.value = true;
  errorMessage.value = '';
  try {
    const { url } = await api.upload('avatars', file);
    avatarUrl.value = url;
    const updated = await authApi.updateProfile({ avatarUrl: url });
    auth.user = updated;
    savedMessage.value = 'อัปเดตรูปโปรไฟล์เรียบร้อยแล้ว';
  } catch (err: any) {
    errorMessage.value = err.message || 'ไม่สามารถอัปโหลดรูปภาพได้';
  } finally {
    isUploadingAvatar.value = false;
  }
}

async function handlePhotoAdd(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  isUploadingPhoto.value = true;
  errorMessage.value = '';
  try {
    const { url } = await api.upload('sitter-documents', file);
    servicePhotos.value = [...servicePhotos.value, url];
  } catch (err: any) {
    errorMessage.value = err.message || 'ไม่สามารถอัปโหลดรูปผลงานได้';
  } finally {
    isUploadingPhoto.value = false;
  }
}

function removePhoto(index: number) {
  servicePhotos.value = servicePhotos.value.filter((_, i) => i !== index);
}

async function handleSave() {
  isSaving.value = true;
  savedMessage.value = '';
  errorMessage.value = '';

  try {
    // 1. Update basic user profile
    const updatedUser = await authApi.updateProfile({
      name: name.value,
      phone: phone.value,
      secondaryPhone: secondaryPhone.value,
      emergencyContactName: emergencyContactName.value,
      avatarUrl: avatarUrl.value ?? undefined,
      address: address.value,
      latitude: latitude.value ?? undefined,
      longitude: longitude.value ?? undefined,
    });
    auth.user = updatedUser;

    // 2. Update sitter service profile
    await sittersApi.updateMine({
      bio: bio.value,
      pricePerDay: pricePerDay.value,
      address: address.value,
      latitude: latitude.value ?? undefined,
      longitude: longitude.value ?? undefined,
      experienceYears: experienceYears.value,
      maxCatsPerDay: maxCatsPerDay.value,
      servicePhotos: servicePhotos.value,
      isAvailable: isAvailable.value,
      socialLine: socialLine.value,
      socialFacebook: socialFacebook.value,
      socialInstagram: socialInstagram.value,
    });

    savedMessage.value = 'บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว';
  } catch (err: any) {
    errorMessage.value = err.message || 'เกิดข้อผิดพลาดในการบันทึกข้อมูล';
  } finally {
    isSaving.value = false;
  }
}

async function handleChangePassword() {
  passwordMessage.value = '';
  passwordError.value = '';

  if (!currentPassword.value) {
    passwordError.value = 'กรุณากรอกรหัสผ่านปัจจุบัน';
    return;
  }
  if (!newPassword.value || newPassword.value.length < 6) {
    passwordError.value = 'รหัสผ่านใหม่ต้องมีความยาวอย่างน้อย 6 ตัวอักษร';
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    passwordError.value = 'รหัสผ่านใหม่และการยืนยันไม่ตรงกัน';
    return;
  }

  isChangingPassword.value = true;
  try {
    const res = await authApi.changePassword({
      currentPassword: currentPassword.value,
      newPassword: newPassword.value,
    });
    passwordMessage.value = res.message || 'เปลี่ยนรหัสผ่านเรียบร้อยแล้ว';
    currentPassword.value = '';
    newPassword.value = '';
    confirmPassword.value = '';
  } catch (err: any) {
    passwordError.value = err.message || 'ไม่สามารถเปลี่ยนรหัสผ่านได้ ตรวจสอบรหัสผ่านเดิมอีกครั้ง';
  } finally {
    isChangingPassword.value = false;
  }
}
</script>

<template>
  <div class="max-w-3xl mx-auto space-y-6 pb-14">
    <!-- Header Banner & Avatar -->
    <div class="bg-white rounded-3xl border border-slate-100 shadow-sm overflow-hidden">
      <!-- Gradient Cover -->
      <div class="h-28 bg-gradient-to-r from-[#1E2238] via-[#2A3152] to-[#FF7A59] relative">
        <div class="absolute inset-0 bg-black/10"></div>
      </div>

      <div class="px-6 pb-6 pt-0 relative">
        <div class="flex flex-col sm:flex-row items-center sm:items-end gap-5 -mt-14 mb-4">
          <!-- Avatar with Camera Badge -->
          <div class="relative shrink-0">
            <label class="block relative cursor-pointer group">
              <div class="w-28 h-28 rounded-2xl bg-white p-1.5 shadow-md">
                <div class="w-full h-full rounded-xl bg-[#FFF0EB] overflow-hidden flex items-center justify-center">
                  <img v-if="resolvedAvatar" :src="resolvedAvatar" class="w-full h-full object-cover" />
                  <User v-else class="w-12 h-12 text-[#1E2238]" />
                </div>
              </div>

              <!-- Camera upload button -->
              <div
                class="absolute bottom-1 right-1 w-8 h-8 rounded-full bg-[#1E2238] text-white flex items-center justify-center shadow-lg border-2 border-white hover:bg-[#FF7A59] transition transform hover:scale-105"
                title="เปลี่ยนรูปโปรไฟล์"
              >
                <Loader2 v-if="isUploadingAvatar" class="w-4 h-4 animate-spin" />
                <Camera v-else class="w-4 h-4" />
              </div>
              <input type="file" accept="image/*" class="hidden" @change="handleAvatarChange" />
            </label>
          </div>

          <!-- User Details -->
          <div class="text-center sm:text-left flex-1 min-w-0">
            <div class="flex flex-wrap items-center justify-center sm:justify-start gap-2 mb-1">
              <h1 class="text-xl font-bold text-slate-800">{{ name || 'ผู้รับฝากเลี้ยง' }}</h1>
              
              <!-- Role Badge -->
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-[#1E2238]/5 text-[#1E2238] border border-[#1E2238]/15">
                🐾 ผู้รับฝากเลี้ยง
              </span>

              <!-- Verification Badge -->
              <span
                v-if="isVerified"
                class="inline-flex items-center gap-1 text-xs text-emerald-700 bg-emerald-50 border border-emerald-200/70 px-2.5 py-0.5 rounded-full font-semibold"
              >
                <ShieldCheck class="w-3.5 h-3.5 text-emerald-600" /> ยืนยันตัวตนแล้ว
              </span>
              <span
                v-else
                class="inline-flex items-center gap-1 text-xs text-amber-700 bg-amber-50 border border-amber-200/70 px-2.5 py-0.5 rounded-full font-semibold"
              >
                <Clock class="w-3.5 h-3.5 text-amber-600" /> รอตรวจสอบตัวตน
              </span>
            </div>
            <p class="text-xs text-slate-500">{{ auth.user?.email }}</p>
          </div>

          <!-- Quick Stats Cards -->
          <div class="flex items-center gap-2.5">
            <div class="px-3.5 py-2 rounded-2xl bg-slate-50 border border-slate-200 text-center min-w-[76px]">
              <div class="flex items-center justify-center gap-1 text-amber-500">
                <Star class="w-3.5 h-3.5 fill-amber-400 text-amber-400" />
                <span class="text-sm font-bold text-slate-800">{{ ratingAvg.toFixed(1) }}</span>
              </div>
              <p class="text-[10px] text-slate-500 mt-0.5">{{ reviewCount }} รีวิว</p>
            </div>

            <div class="px-3.5 py-2 rounded-2xl bg-slate-50 border border-slate-200 text-center min-w-[76px]">
              <p class="text-sm font-bold text-slate-800">{{ jobCount }}</p>
              <p class="text-[10px] text-slate-500 mt-0.5">งานดูแล</p>
            </div>

            <div class="px-3.5 py-2 rounded-2xl bg-[#FFF0EB] border border-[#FF7A59]/30 text-center min-w-[90px]">
              <p class="text-sm font-bold text-[#FF7A59]">฿{{ (auth.user?.walletBalance ?? 0).toLocaleString() }}</p>
              <button
                type="button"
                class="text-[10px] font-semibold text-[#E05638] hover:underline mt-0.5 flex items-center justify-center gap-0.5 w-full"
                @click="showPayoutModal = true"
              >
                <Wallet class="w-2.5 h-2.5" /> ถอนเงิน
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Alert Messages -->
    <div v-if="savedMessage" class="p-4 rounded-2xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-800 flex items-center gap-2 shadow-sm">
      <CheckCircle2 class="w-5 h-5 text-emerald-600 shrink-0" />
      <span>{{ savedMessage }}</span>
    </div>
    <div v-if="errorMessage" class="p-4 rounded-2xl bg-red-50 border border-red-200 text-sm text-red-700 flex items-center gap-2 shadow-sm">
      <AlertCircle class="w-5 h-5 text-red-500 shrink-0" />
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Loading skeleton -->
    <div v-if="isLoading" class="bg-white rounded-3xl p-12 text-center text-slate-400 border border-slate-100 shadow-sm">
      <Loader2 class="w-8 h-8 animate-spin mx-auto mb-3 text-[#FF7A59]" />
      <p class="text-sm">กำลังโหลดข้อมูลโปรไฟล์...</p>
    </div>

    <div v-else class="space-y-6">
      <!-- 1. Availability Status Toggle Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6">
        <div class="flex items-center justify-between gap-4">
          <div class="flex items-center gap-3">
            <div
              :class="[
                'w-11 h-11 rounded-2xl flex items-center justify-center shrink-0 transition',
                isAvailable ? 'bg-emerald-50 text-emerald-600' : 'bg-slate-100 text-slate-400'
              ]"
            >
              <Sparkles class="w-5 h-5" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h3 class="text-base font-bold text-slate-800">สถานะเปิดรับงาน</h3>
                <span
                  :class="[
                    'px-2 py-0.5 rounded-full text-[11px] font-semibold',
                    isAvailable ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-500'
                  ]"
                >
                  {{ isAvailable ? '🟢 พร้อมรับงาน' : '⚪ พักรับงานชั่วคราว' }}
                </span>
              </div>
              <p class="text-xs text-slate-500 mt-0.5">
                {{ isAvailable ? 'โปรไฟล์ของคุณจะแสดงในผลการค้นหา และลูกค้าสามารถส่งคำขอจองได้' : 'โปรไฟล์จะถูกซ่อนจากผลการค้นหาชั่วคราว' }}
              </p>
            </div>
          </div>

          <!-- Switch Toggle -->
          <label class="relative inline-flex items-center cursor-pointer shrink-0">
            <input v-model="isAvailable" type="checkbox" class="sr-only peer" />
            <div class="w-13 h-7 bg-slate-200 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[3px] after:left-[4px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5.5 after:w-5.5 after:transition-all peer-checked:bg-emerald-500"></div>
          </label>
        </div>
      </div>

      <!-- 2. Personal Information Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-4">
        <div class="flex items-center gap-2.5 pb-3 border-b border-slate-100">
          <User class="w-5 h-5 text-[#FF7A59]" />
          <div>
            <h3 class="font-bold text-slate-800 text-sm">ข้อมูลส่วนตัวพี่เลี้ยง</h3>
            <p class="text-xs text-slate-400">ชื่อและข้อมูลติดต่อสำหรับประสานงานกับลูกค้า</p>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ชื่อ - นามสกุล</label>
            <input
              v-model="name"
              type="text"
              placeholder="ระบุชื่อของคุณ"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">เบอร์โทรศัพท์หลัก</label>
            <input
              v-model="phone"
              type="tel"
              placeholder="08x-xxx-xxxx"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">เบอร์โทรศัพท์สำรอง</label>
            <input
              v-model="secondaryPhone"
              type="tel"
              placeholder="เบอร์โทรเพิ่มเติม (ถ้ามี)"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ชื่อผู้ติดต่อฉุกเฉิน</label>
            <input
              v-model="emergencyContactName"
              type="text"
              placeholder="เช่น คุณแม่ / ผู้ดูแลร่วม"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>
        </div>
      </div>

      <!-- 3. Service & Pricing Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-4">
        <div class="flex items-center gap-2.5 pb-3 border-b border-slate-100">
          <Briefcase class="w-5 h-5 text-[#FF7A59]" />
          <div>
            <h3 class="font-bold text-slate-800 text-sm">ข้อมูลบริการและการคิดค่าบริการ</h3>
            <p class="text-xs text-slate-400">กำหนดอัตราค่าบริการและทักษะความเชี่ยวชาญของคุณ</p>
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1.5">คำแนะนำตัวและประสบการณ์ (Bio)</label>
          <textarea
            v-model="bio"
            rows="3"
            placeholder="แนะนำตัวเอง ความรักแมว ทักษะพิเศษ เช่น ป้อนยาได้ แปรงขน อาบน้ำ ให้สารน้ำ ฯลฯ"
            class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
          ></textarea>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ราคาต่อวัน (บาท)</label>
            <div class="relative">
              <span class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400 text-sm">฿</span>
              <input
                v-model.number="pricePerDay"
                type="number"
                min="0"
                class="w-full pl-8 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm font-semibold focus:outline-none focus:border-[#FF7A59]"
              />
            </div>
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ประสบการณ์ (ปี)</label>
            <input
              v-model.number="experienceYears"
              type="number"
              min="0"
              placeholder="1"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">รับดูแลสูงสุด (ตัว/วัน)</label>
            <input
              v-model.number="maxCatsPerDay"
              type="number"
              min="1"
              placeholder="3"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>
        </div>
      </div>

      <!-- 4. Location & GPS Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-4">
        <div class="flex items-center gap-2.5 pb-3 border-b border-slate-100">
          <MapPin class="w-5 h-5 text-[#FF7A59]" />
          <div>
            <h3 class="font-bold text-slate-800 text-sm">พื้นที่ให้บริการและพิกัด</h3>
            <p class="text-xs text-slate-400">ระบบจะใช้พิกัดนี้คำนวณระยะห่างเพื่อแนะนำให้ลูกค้าที่อยู่ใกล้เคียง</p>
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-slate-700 mb-1.5">ที่อยู่ / ย่านที่ให้บริการ</label>
          <input
            v-model="address"
            type="text"
            placeholder="เช่น บางนา, สุขุมวิท 101, คอนโด Ideo..."
            class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
          />
        </div>

        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pt-1">
          <button
            type="button"
            class="inline-flex items-center gap-1.5 text-xs font-semibold text-[#FF7A59] hover:text-[#E05638] bg-[#FFF0EB] hover:bg-[#FFE3D9] px-3.5 py-2 rounded-xl transition w-fit"
            :disabled="isLocating"
            @click="useMyLocation"
          >
            <Loader2 v-if="isLocating" class="w-3.5 h-3.5 animate-spin" />
            <MapPin v-else class="w-3.5 h-3.5" />
            {{ isLocating ? 'กำลังค้นหาพิกัด...' : 'ดึงพิกัดตำแหน่งปัจจุบัน (GPS)' }}
          </button>

          <div v-if="latitude && longitude" class="text-xs text-slate-500 flex items-center gap-2">
            <span class="inline-block w-2 h-2 rounded-full bg-emerald-500"></span>
            พิกัด: {{ latitude.toFixed(4) }}, {{ longitude.toFixed(4) }}
          </div>
        </div>
      </div>

      <!-- 5. Social & Contacts Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-4">
        <div class="flex items-center gap-2.5 pb-3 border-b border-slate-100">
          <Phone class="w-5 h-5 text-[#FF7A59]" />
          <div>
            <h3 class="font-bold text-slate-800 text-sm">ช่องทางโซเชียลมีเดีย</h3>
            <p class="text-xs text-slate-400">ช่องทางติดต่อเพิ่มเติมให้ลูกค้ามั่นใจในตัวคุณ</p>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label class="block text-[11px] font-semibold text-slate-500 mb-1">Line ID</label>
            <input
              v-model="socialLine"
              type="text"
              placeholder="@yourlineid"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-[11px] font-semibold text-slate-500 mb-1">Facebook</label>
            <input
              v-model="socialFacebook"
              type="text"
              placeholder="ชื่อเพจหรือโปรไฟล์"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-[11px] font-semibold text-slate-500 mb-1">Instagram</label>
            <input
              v-model="socialInstagram"
              type="text"
              placeholder="@instagram"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>
        </div>
      </div>

      <!-- 6. Portfolio & Service Photos Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-4">
        <div class="flex items-center justify-between pb-3 border-b border-slate-100">
          <div class="flex items-center gap-2.5">
            <Sparkles class="w-5 h-5 text-[#FF7A59]" />
            <div>
              <h3 class="font-bold text-slate-800 text-sm">รูปถ่ายสถานที่และผลงานดูแลแมว</h3>
              <p class="text-xs text-slate-400">อัปโหลดรูปบ้าน คอนโด หรือผลงานที่เคยดูแลน้องแมว</p>
            </div>
          </div>
          <span class="text-xs text-slate-400">{{ servicePhotos.length }} รูป</span>
        </div>

        <div class="grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 gap-3">
          <!-- Photos list -->
          <div
            v-for="(photo, i) in servicePhotos"
            :key="i"
            class="relative aspect-square rounded-2xl overflow-hidden border border-slate-200 group bg-slate-100"
          >
            <img :src="resolveImageUrl(photo)" class="w-full h-full object-cover" />
            <button
              type="button"
              class="absolute top-1.5 right-1.5 w-6 h-6 rounded-full bg-red-500 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition shadow-md"
              title="ลบรูปภาพ"
              @click="removePhoto(i)"
            >
              <Trash2 class="w-3.5 h-3.5" />
            </button>
          </div>

          <!-- Add Photo Button -->
          <label
            class="aspect-square rounded-2xl border-2 border-dashed border-slate-200 hover:border-[#FF7A59] hover:bg-[#FFF0EB]/30 transition flex flex-col items-center justify-center cursor-pointer text-slate-400 hover:text-[#FF7A59]"
          >
            <Loader2 v-if="isUploadingPhoto" class="w-5 h-5 animate-spin" />
            <template v-else>
              <Plus class="w-5 h-5 mb-1" />
              <span class="text-[11px] font-semibold">เพิ่มรูป</span>
            </template>
            <input type="file" accept="image/*" class="hidden" @change="handlePhotoAdd" />
          </label>
        </div>
      </div>

      <!-- 7. Save Changes Button -->
      <div class="pt-2">
        <button
          :disabled="isSaving"
          class="w-full py-3.5 rounded-2xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-bold text-sm flex items-center justify-center gap-2 shadow-md hover:shadow-lg transition disabled:opacity-60"
          @click="handleSave"
        >
          <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
          <span>บันทึกการเปลี่ยนแปลงทั้งหมด</span>
        </button>
      </div>

      <!-- 8. Password Security Card -->
      <div class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-4">
        <div class="flex items-center gap-2.5 pb-3 border-b border-slate-100">
          <KeyRound class="w-5 h-5 text-slate-600" />
          <div>
            <h3 class="font-bold text-slate-800 text-sm">เปลี่ยนรหัสผ่าน</h3>
            <p class="text-xs text-slate-400">ความปลอดภัยของบัญชีผู้ใช้งาน</p>
          </div>
        </div>

        <div v-if="passwordMessage" class="p-3.5 rounded-2xl bg-emerald-50 border border-emerald-200 text-xs text-emerald-800 flex items-center gap-2">
          <CheckCircle2 class="w-4 h-4 text-emerald-600 shrink-0" />
          <span>{{ passwordMessage }}</span>
        </div>
        <div v-if="passwordError" class="p-3.5 rounded-2xl bg-red-50 border border-red-200 text-xs text-red-700 flex items-center gap-2">
          <AlertCircle class="w-4 h-4 text-red-500 shrink-0" />
          <span>{{ passwordError }}</span>
        </div>

        <div class="space-y-3 max-w-md">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">รหัสผ่านปัจจุบัน</label>
            <input
              v-model="currentPassword"
              type="password"
              placeholder="••••••••"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">รหัสผ่านใหม่ (อย่างน้อย 6 ตัวอักษร)</label>
            <input
              v-model="newPassword"
              type="password"
              placeholder="••••••••"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">ยืนยันรหัสผ่านใหม่</label>
            <input
              v-model="confirmPassword"
              type="password"
              placeholder="••••••••"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
            />
          </div>

          <button
            type="button"
            :disabled="isChangingPassword || !currentPassword || !newPassword"
            class="px-5 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-900 text-white font-semibold text-xs flex items-center justify-center gap-2 disabled:opacity-40 transition"
            @click="handleChangePassword"
          >
            <Loader2 v-if="isChangingPassword" class="w-3.5 h-3.5 animate-spin" />
            <span>อัปเดตรหัสผ่าน</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Payout Modal -->
    <PayoutModal
      :is-open="showPayoutModal"
      :wallet-balance="auth.user?.walletBalance ?? 0"
      @close="showPayoutModal = false"
      @submitted="() => { showPayoutModal = false; auth.checkSession(); }"
    />
  </div>
</template>
