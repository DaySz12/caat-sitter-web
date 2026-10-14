<script setup lang="ts">
import { ref, onMounted } from 'vue';
import {
  User,
  Loader2,
  CheckCircle2,
  MapPin,
  Lock,
  Phone,
  MessageSquare,
  Shield,
  Cat as CatIcon,
  Calendar,
  AlertCircle,
  Camera,
  KeyRound,
  ExternalLink,
  Sparkles
} from 'lucide-vue-next';
import { RouterLink } from 'vue-router';
import { useAuthStore } from '@/features/auth/auth.store';
import { authApi } from '@/features/auth/auth.api';
import { catsApi } from '@/features/cats/cats.api';
import { bookingApi } from '@/features/booking/booking.api';
import { api } from '@/shared/lib/api';
import { formatCurrency, formatDate } from '@/shared/lib/format';

const auth = useAuthStore();

// Form state
const name = ref('');
const phone = ref('');
const lineId = ref('');
const secondaryPhone = ref('');
const emergencyContactName = ref('');
const address = ref('');
const buildingNote = ref('');
const latitude = ref<number | null>(null);
const longitude = ref<number | null>(null);
const avatarUrl = ref<string | null>(null);

// Stats
const catCount = ref(0);
const bookingCount = ref(0);

// Statuses
const isLoading = ref(true);
const isSaving = ref(false);
const isUploading = ref(false);
const isLocating = ref(false);
const savedMessage = ref('');
const errorMessage = ref('');

// Password change state
const currentPassword = ref('');
const newPassword = ref('');
const confirmPassword = ref('');
const isChangingPassword = ref(false);
const passwordMessage = ref('');
const passwordError = ref('');

onMounted(async () => {
  if (!auth.user) return;
  name.value = auth.user.name ?? '';
  phone.value = auth.user.phone ?? '';
  avatarUrl.value = auth.user.avatarUrl ?? null;
  address.value = auth.user.address ?? '';
  buildingNote.value = auth.user.buildingNote ?? '';
  latitude.value = auth.user.latitude ?? null;
  longitude.value = auth.user.longitude ?? null;
  lineId.value = auth.user.lineId ?? '';
  secondaryPhone.value = auth.user.secondaryPhone ?? '';
  emergencyContactName.value = auth.user.emergencyContactName ?? '';

  try {
    const [cats, bookings] = await Promise.all([
      catsApi.mine().catch(() => []),
      bookingApi.mine('all').catch(() => []),
    ]);
    catCount.value = cats.length;
    bookingCount.value = bookings.length;
  } catch (e) {
    console.error('Failed to load stats', e);
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
    (err) => {
      isLocating.value = false;
      errorMessage.value = 'ไม่สามารถดึงตำแหน่งปัจจุบันได้: ' + err.message;
    },
    { enableHighAccuracy: true, timeout: 10000 }
  );
}

async function handleAvatarChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  isUploading.value = true;
  errorMessage.value = '';
  try {
    const { url } = await api.upload('avatars', file);
    avatarUrl.value = url;
    // Auto-save avatar
    auth.user = await authApi.updateProfile({ avatarUrl: url });
    savedMessage.value = 'อัปเดตรูปโปรไฟล์เรียบร้อยแล้ว';
  } catch (err: any) {
    errorMessage.value = err.message || 'อัปโหลดรูปไม่สำเร็จ';
  } finally {
    isUploading.value = false;
  }
}

async function handleSaveProfile() {
  isSaving.value = true;
  savedMessage.value = '';
  errorMessage.value = '';
  try {
    auth.user = await authApi.updateProfile({
      name: name.value.trim(),
      phone: phone.value.trim() || undefined,
      lineId: lineId.value.trim() || undefined,
      secondaryPhone: secondaryPhone.value.trim() || undefined,
      emergencyContactName: emergencyContactName.value.trim() || undefined,
      address: address.value.trim() || undefined,
      buildingNote: buildingNote.value.trim() || undefined,
      latitude: latitude.value ?? undefined,
      longitude: longitude.value ?? undefined,
      avatarUrl: avatarUrl.value ?? undefined,
    });
    savedMessage.value = 'บันทึกข้อมูลโปรไฟล์เรียบร้อยแล้ว';
    setTimeout(() => {
      savedMessage.value = '';
    }, 4000);
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
  <div class="max-w-3xl mx-auto space-y-6 pb-12">
    <!-- Header Banner & Avatar -->
    <div class="bg-white rounded-3xl border border-slate-100 shadow-sm overflow-hidden">
      <div class="h-28 bg-gradient-to-r from-[#FF7A59] via-[#FF8A65] to-[#FFA07A] relative">
        <div class="absolute inset-0 bg-black/5"></div>
      </div>

      <div class="px-6 pb-6 pt-0 relative">
        <div class="flex flex-col sm:flex-row items-center sm:items-end gap-5 -mt-14 mb-4">
          <!-- Avatar with Camera Badge -->
          <div class="relative shrink-0">
            <label class="block relative cursor-pointer group">
              <div class="w-28 h-28 rounded-2xl bg-white p-1.5 shadow-md">
                <div class="w-full h-full rounded-xl bg-[#FFF0EB] overflow-hidden flex items-center justify-center">
                  <img v-if="avatarUrl" :src="avatarUrl" class="w-full h-full object-cover" />
                  <User v-else class="w-12 h-12 text-[#FF7A59]" />
                </div>
              </div>

              <!-- Camera Badge button -->
              <div
                class="absolute bottom-1 right-1 w-8 h-8 rounded-full bg-[#FF7A59] text-white flex items-center justify-center shadow-lg border-2 border-white hover:bg-[#E05638] transition transform hover:scale-105"
                title="เปลี่ยนรูปโปรไฟล์"
              >
                <Loader2 v-if="isUploading" class="w-4 h-4 animate-spin" />
                <Camera v-else class="w-4 h-4" />
              </div>
              <input type="file" accept="image/*" class="hidden" @change="handleAvatarChange" />
            </label>
          </div>

          <!-- User Details -->
          <div class="text-center sm:text-left flex-1 min-w-0">
            <div class="flex flex-wrap items-center justify-center sm:justify-start gap-2 mb-1">
              <h1 class="text-xl font-bold text-slate-800">{{ name || 'เจ้าของสัตว์เลี้ยง' }}</h1>
              <span class="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-orange-50 text-[#FF7A59] border border-orange-200">
                <CatIcon class="w-3 h-3" /> เจ้าของสัตว์เลี้ยง
              </span>
            </div>
            <p class="text-xs text-slate-500">{{ auth.user?.email }}</p>
          </div>

          <!-- Quick Stats -->
          <div class="flex items-center gap-3">
            <RouterLink
              to="/owner/cats"
              class="px-4 py-2 rounded-2xl bg-slate-50 hover:bg-orange-50 border border-slate-200 hover:border-orange-200 transition text-center group"
            >
              <p class="text-xs text-slate-500 group-hover:text-[#FF7A59]">แมวของฉัน</p>
              <p class="text-lg font-bold text-slate-800 group-hover:text-[#FF7A59]">{{ catCount }} ตัว</p>
            </RouterLink>
            <RouterLink
              to="/owner/bookings"
              class="px-4 py-2 rounded-2xl bg-slate-50 hover:bg-orange-50 border border-slate-200 hover:border-orange-200 transition text-center group"
            >
              <p class="text-xs text-slate-500 group-hover:text-[#FF7A59]">การจอง</p>
              <p class="text-lg font-bold text-slate-800 group-hover:text-[#FF7A59]">{{ bookingCount }} ครั้ง</p>
            </RouterLink>
          </div>
        </div>
      </div>
    </div>

    <!-- Alert Messages -->
    <div
      v-if="savedMessage"
      class="p-4 rounded-2xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center gap-2 shadow-sm animate-fadeIn"
    >
      <CheckCircle2 class="w-5 h-5 shrink-0 text-emerald-600" />
      <span>{{ savedMessage }}</span>
    </div>

    <div
      v-if="errorMessage"
      class="p-4 rounded-2xl bg-rose-50 border border-rose-200 text-sm text-rose-700 flex items-center gap-2 shadow-sm animate-fadeIn"
    >
      <AlertCircle class="w-5 h-5 shrink-0 text-rose-600" />
      <span>{{ errorMessage }}</span>
    </div>

    <!-- Form Container -->
    <div class="space-y-6">
      <!-- Section 1: ข้อมูลส่วนตัวและการติดต่อ -->
      <section class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-slate-100">
          <div class="w-8 h-8 rounded-xl bg-orange-100 text-[#FF7A59] flex items-center justify-center">
            <User class="w-4 h-4" />
          </div>
          <div>
            <h2 class="text-base font-bold text-slate-800">ข้อมูลส่วนตัวและการติดต่อ</h2>
            <p class="text-xs text-slate-500">ข้อมูลนี้จะช่วยให้พี่เลี้ยงและเจ้าหน้าที่ติดต่อคุณได้สะดวกเมื่อรับงาน</p>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ชื่อ-นามสกุล *</label>
            <input
              v-model="name"
              type="text"
              placeholder="ระบุชื่อและนามสกุล"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">อีเมล (บัญชีผู้ใช้)</label>
            <input
              :value="auth.user?.email"
              type="email"
              disabled
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm bg-slate-50 text-slate-400 cursor-not-allowed"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5 flex items-center gap-1.5">
              <Phone class="w-3.5 h-3.5 text-slate-400" /> เบอร์โทรศัพท์หลัก *
            </label>
            <input
              v-model="phone"
              type="tel"
              placeholder="เช่น 081-234-5678"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5 flex items-center gap-1.5">
              <MessageSquare class="w-3.5 h-3.5 text-emerald-500" /> LINE ID
            </label>
            <input
              v-model="lineId"
              type="text"
              placeholder="เช่น mycat_lineid (สะดวกในการส่งรูปน้องแมว)"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ชื่อผู้ติดต่อสำรอง / ฉุกเฉิน</label>
            <input
              v-model="emergencyContactName"
              type="text"
              placeholder="เช่น แฟน, รูมเมท, ญาติ"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">เบอร์โทรศัพท์สำรอง / ฉุกเฉิน</label>
            <input
              v-model="secondaryPhone"
              type="tel"
              placeholder="เช่น 089-987-6543 (ติดต่อเมื่อโทรไม่ติด)"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>
        </div>
      </section>

      <!-- Section 2: สถานที่นัดหมายบริการเริ่มต้น -->
      <section class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-slate-100">
          <div class="flex items-center gap-2">
            <div class="w-8 h-8 rounded-xl bg-orange-100 text-[#FF7A59] flex items-center justify-center">
              <MapPin class="w-4 h-4" />
            </div>
            <div>
              <h2 class="text-base font-bold text-slate-800">สถานที่นัดหมายบริการเริ่มต้น (Default Address)</h2>
              <p class="text-xs text-slate-500">ที่อยู่และวิธีเข้าอาคารที่จะนำไปกรอกในหน้าจองให้อัตโนมัติ</p>
            </div>
          </div>

          <button
            type="button"
            :disabled="isLocating"
            class="px-3 py-1.5 rounded-xl border border-orange-200 text-[#FF7A59] hover:bg-orange-50 text-xs font-medium flex items-center gap-1.5 transition disabled:opacity-50"
            @click="useMyLocation"
          >
            <Loader2 v-if="isLocating" class="w-3.5 h-3.5 animate-spin" />
            <MapPin v-else class="w-3.5 h-3.5" />
            <span>ใช้พิกัดปัจจุบัน (GPS)</span>
          </button>
        </div>

        <div class="space-y-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ที่อยู่บ้าน / คอนโด / หมู่บ้าน</label>
            <textarea
              v-model="address"
              rows="3"
              placeholder="ระบุบ้านเลขที่, คอนโด/หมู่บ้าน, ซอย, ถนน, แขวง/ตำบล, เขต/อำเภอ, จังหวัด, รหัสไปรษณีย์"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition leading-relaxed"
            ></textarea>
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">วิธีเข้าอาคาร / จุดสังเกตการเดินทาง</label>
            <textarea
              v-model="buildingNote"
              rows="2"
              placeholder="เช่น ฝากคีย์การ์ดไว้ที่นิติบุคคลชั้น 1, รหัสผ่านประตูดิจิทัล 4589#, มีที่จอดรถหน้าตึก B"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition leading-relaxed"
            ></textarea>
          </div>

          <div v-if="latitude && longitude" class="p-3 rounded-xl bg-orange-50/60 border border-orange-100 flex items-center justify-between text-xs text-slate-600">
            <span class="flex items-center gap-1.5">
              <MapPin class="w-3.5 h-3.5 text-[#FF7A59]" />
              พิกัด GPS: {{ latitude.toFixed(4) }}, {{ longitude.toFixed(4) }}
            </span>
            <a
              :href="`https://www.google.com/maps/search/?api=1&query=${latitude},${longitude}`"
              target="_blank"
              class="text-[#FF7A59] hover:underline flex items-center gap-1 font-medium"
            >
              ดูบนแผนที่ <ExternalLink class="w-3 h-3" />
            </a>
          </div>

          <div class="p-3.5 rounded-2xl bg-amber-50/80 border border-amber-200/60 text-xs text-amber-800 flex items-start gap-2.5">
            <Sparkles class="w-4 h-4 shrink-0 text-amber-600 mt-0.5" />
            <p class="leading-relaxed">
              เมื่อคุณบันทึกที่อยู่เริ่มต้นนี้ไว้ ทุกครั้งที่คุณกดจองบริการพี่เลี้ยง ระบบจะดึงที่อยู่นี้ไปกรอกในแบบฟอร์มการจองให้อัตโนมัติทันที
            </p>
          </div>
        </div>
      </section>

      <!-- Save Profile Button -->
      <div class="flex justify-end">
        <button
          type="button"
          :disabled="isSaving"
          class="w-full sm:w-auto px-8 py-3 rounded-2xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm flex items-center justify-center gap-2 shadow-sm transition disabled:opacity-60"
          @click="handleSaveProfile"
        >
          <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
          <span>บันทึกการเปลี่ยนแปลงทั้งหมด</span>
        </button>
      </div>

      <!-- Section 3: ความปลอดภัยของบัญชี (เปลี่ยนรหัสผ่าน) -->
      <section class="bg-white rounded-3xl border border-slate-100 shadow-sm p-6">
        <div class="flex items-center gap-2 mb-4 pb-3 border-b border-slate-100">
          <div class="w-8 h-8 rounded-xl bg-slate-100 text-slate-600 flex items-center justify-center">
            <KeyRound class="w-4 h-4" />
          </div>
          <div>
            <h2 class="text-base font-bold text-slate-800">ความปลอดภัยของบัญชี</h2>
            <p class="text-xs text-slate-500">เปลี่ยนรหัสผ่านสำหรับเข้าสู่ระบบ</p>
          </div>
        </div>

        <div v-if="passwordMessage" class="mb-4 p-3 rounded-xl bg-emerald-50 border border-emerald-200 text-xs text-emerald-700 flex items-center gap-2">
          <CheckCircle2 class="w-4 h-4 text-emerald-600 shrink-0" />
          <span>{{ passwordMessage }}</span>
        </div>

        <div v-if="passwordError" class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-xs text-rose-700 flex items-center gap-2">
          <AlertCircle class="w-4 h-4 text-rose-600 shrink-0" />
          <span>{{ passwordError }}</span>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">รหัสผ่านปัจจุบัน</label>
            <input
              v-model="currentPassword"
              type="password"
              placeholder="รหัสผ่านปัจจุบัน"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">รหัสผ่านใหม่</label>
            <input
              v-model="newPassword"
              type="password"
              placeholder="อย่างน้อย 6 ตัวอักษร"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ยืนยันรหัสผ่านใหม่</label>
            <input
              v-model="confirmPassword"
              type="password"
              placeholder="ยืนยันรหัสผ่านใหม่อีกครั้ง"
              class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:ring-2 focus:ring-[#FF7A59]/20 focus:border-[#FF7A59] outline-none transition"
            />
          </div>
        </div>

        <div class="flex justify-end mt-4">
          <button
            type="button"
            :disabled="isChangingPassword || !currentPassword || !newPassword"
            class="px-5 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-900 text-white font-medium text-xs flex items-center justify-center gap-2 transition disabled:opacity-50"
            @click="handleChangePassword"
          >
            <Loader2 v-if="isChangingPassword" class="w-3.5 h-3.5 animate-spin" />
            <Lock class="w-3.5 h-3.5" />
            <span>อัปเดตรหัสผ่าน</span>
          </button>
        </div>
      </section>
    </div>
  </div>
</template>
