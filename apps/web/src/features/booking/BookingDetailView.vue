<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  ArrowLeft, MessageCircle, Phone, CalendarDays, Cat as CatIcon, HeartPulse,
  Utensils, Star, CreditCard, CheckCircle2, Clock, AlertCircle, Upload, Eye, X, Copy, Check
} from 'lucide-vue-next';
import { bookingApi } from './booking.api';
import { careApi } from '@/features/care/care.api';
import { chatApi } from '@/features/chat/chat.api';
import { API_URL, api } from '@/shared/lib/api';
import { useAuthStore } from '@/features/auth/auth.store';
import { useSummaryStore } from '@/features/notifications/summary.store';
import CarePanel from '@/features/care/CarePanel.vue';
import ReviewForm from '@/features/reviews/ReviewForm.vue';
import type { BookingCareDto, BookingDto, BookingStatus, ReviewDto } from '@/shared/types/models';
import { ageDisplay, formatCurrency, formatDateRange, formatRelativeTime } from '@/shared/lib/format';
import { roleBase } from '@/shared/lib/paths';
import { useLiveEvent } from '@/shared/lib/liveEvents';
import StatusBadge from '@/shared/ui/StatusBadge.vue';
import PhotoStrip from '@/shared/ui/PhotoStrip.vue';
import RatingStars from '@/shared/ui/RatingStars.vue';
import UserAvatar from '@/shared/ui/UserAvatar.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const summaryStore = useSummaryStore();

const booking = ref<BookingDto | null>(null);
const care = ref<BookingCareDto | null>(null);
const isLoading = ref(true);
const isBusy = ref(false);
const error = ref<string | null>(null);

// Payment modal state
const showPaymentModal = ref(false);
const showSlipModal = ref(false);
const selectedSlipFile = ref<File | null>(null);
const slipPreviewUrl = ref<string | null>(null);
const isUploadingSlip = ref(false);
const slipError = ref<string | null>(null);
const copiedField = ref<string | null>(null);

const isSitter = computed(() => auth.user?.role === 'sitter');
const other = computed(() => (isSitter.value ? booking.value?.owner : booking.value?.sitter));
// Past bookings only show the care log when one was actually kept.
const showCare = computed(() => {
  if (!booking.value || !care.value) return false;
  if (booking.value.status === 'confirmed' || booking.value.status === 'in_progress') return true;
  return booking.value.status === 'completed' && (care.value.attendance.length > 0 || care.value.checklists.length > 0);
});

async function load() {
  const id = route.params.id as string;
  [booking.value, care.value] = await Promise.all([bookingApi.getById(id), careApi.get(id)]);
  isLoading.value = false;
}

async function updateStatus(status: BookingStatus, confirmText?: string) {
  if (!booking.value || (confirmText && !confirm(confirmText))) return;
  isBusy.value = true;
  error.value = null;
  try {
    const updated = await bookingApi.updateStatus(booking.value.id, status);
    booking.value = { ...updated, review: booking.value.review };
    care.value = await careApi.get(updated.id);
    summaryStore.refresh();
  } catch (e) {
    error.value = (e as Error).message;
  } finally {
    isBusy.value = false;
  }
}

async function openChat() {
  if (!booking.value || !other.value) return;
  const room = await chatApi.start(other.value.id, booking.value.id);
  router.push(`${roleBase(auth.user?.role)}/chat/${room.id}`);
}

function onCareChanged(updated: BookingCareDto) {
  care.value = updated;
  // A first check-in moves a confirmed booking to in_progress.
  if (booking.value?.status === 'confirmed' && updated.attendance.length) booking.value.status = 'in_progress';
}

function onReviewed(review: ReviewDto) {
  if (booking.value) booking.value.review = review;
}

function fullSlipUrl(url: string | null) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) return url;
  return `${API_URL}${url}`;
}

function copyText(text: string, field: string) {
  navigator.clipboard.writeText(text);
  copiedField.value = field;
  setTimeout(() => {
    if (copiedField.value === field) copiedField.value = null;
  }, 2000);
}

function openPaymentModal() {
  selectedSlipFile.value = null;
  slipPreviewUrl.value = null;
  slipError.value = null;
  showPaymentModal.value = true;
}

function onSlipFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  if (!file.type.startsWith('image/')) {
    slipError.value = 'กรุณาเลือกไฟล์รูปภาพเท่านั้น (JPG, PNG, WebP)';
    return;
  }
  if (file.size > 10 * 1024 * 1024) {
    slipError.value = 'ขนาดไฟล์ต้องไม่เกิน 10MB';
    return;
  }

  slipError.value = null;
  selectedSlipFile.value = file;
  slipPreviewUrl.value = URL.createObjectURL(file);
}

function removeSlipFile() {
  selectedSlipFile.value = null;
  slipPreviewUrl.value = null;
}

function createDemoSlipFile(): File {
  const canvas = document.createElement('canvas');
  canvas.width = 400;
  canvas.height = 560;
  const ctx = canvas.getContext('2d')!;

  ctx.fillStyle = '#065f46';
  ctx.fillRect(0, 0, 400, 90);
  ctx.fillStyle = '#f8fafc';
  ctx.fillRect(0, 90, 400, 470);

  ctx.fillStyle = '#ffffff';
  ctx.font = 'bold 20px sans-serif';
  ctx.fillText('โอนเงินสำเร็จ', 140, 42);
  ctx.font = '13px sans-serif';
  ctx.fillText('PromptPay CatSitter Service', 115, 66);

  ctx.fillStyle = '#1e293b';
  ctx.font = 'bold 15px sans-serif';
  ctx.fillText('ยอดเงินที่โอน', 40, 140);
  ctx.font = 'bold 28px sans-serif';
  ctx.fillStyle = '#059669';
  ctx.fillText(`฿${booking.value?.totalPrice?.toLocaleString('th-TH') ?? 0}`, 40, 180);

  ctx.strokeStyle = '#e2e8f0';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(40, 210);
  ctx.lineTo(360, 210);
  ctx.stroke();

  ctx.fillStyle = '#64748b';
  ctx.font = '13px sans-serif';
  ctx.fillText('ไปยัง:', 40, 245);
  ctx.fillStyle = '#1e293b';
  ctx.font = 'bold 14px sans-serif';
  ctx.fillText('บริษัท แคท ซิตเตอร์ จำกัด', 40, 268);
  ctx.font = '13px sans-serif';
  ctx.fillStyle = '#475569';
  ctx.fillText('พร้อมเพย์ 010-5-56600-0000', 40, 290);

  ctx.fillStyle = '#64748b';
  ctx.fillText('รหัสการจอง:', 40, 335);
  ctx.fillStyle = '#1e293b';
  ctx.font = 'bold 14px sans-serif';
  ctx.fillText(booking.value?.bookingCode ?? 'BK-DEMO', 40, 358);

  ctx.fillStyle = '#64748b';
  ctx.font = '13px sans-serif';
  ctx.fillText('วันเวลาที่ทำรายการ:', 40, 405);
  ctx.fillStyle = '#1e293b';
  ctx.fillText(new Date().toLocaleString('th-TH'), 40, 428);

  ctx.fillStyle = '#059669';
  ctx.fillRect(40, 480, 320, 40);
  ctx.fillStyle = '#ffffff';
  ctx.font = 'bold 13px sans-serif';
  ctx.fillText('✓ ตรวจสอบข้อมูลสลิปเรียบร้อย', 95, 505);

  const dataUrl = canvas.toDataURL('image/png');
  const arr = dataUrl.split(',');
  const bstr = atob(arr[1]);
  let n = bstr.length;
  const u8arr = new Uint8Array(n);
  while (n--) {
    u8arr[n] = bstr.charCodeAt(n);
  }
  return new File([u8arr], `slip-${booking.value?.bookingCode ?? 'test'}.png`, { type: 'image/png' });
}

function useDemoSlip() {
  const file = createDemoSlipFile();
  selectedSlipFile.value = file;
  slipPreviewUrl.value = URL.createObjectURL(file);
}

async function submitPaymentSlip() {
  if (!selectedSlipFile.value || !booking.value) return;
  isUploadingSlip.value = true;
  slipError.value = null;
  try {
    const uploadRes = await api.upload('payment-slips', selectedSlipFile.value);
    const updated = await bookingApi.uploadPaymentSlip(booking.value.id, uploadRes.url);
    booking.value = { ...updated, review: booking.value.review };
    showPaymentModal.value = false;
    selectedSlipFile.value = null;
    slipPreviewUrl.value = null;
    summaryStore.refresh();
  } catch (err: any) {
    slipError.value = err?.message || 'ไม่สามารถอัปโหลดสลิปได้ กรุณาลองใหม่อีกครั้ง';
  } finally {
    isUploadingSlip.value = false;
  }
}

// Live updates from the other side (check-ins, ticked tasks, status changes) refresh the page.
function reloadIfShown(bookingId: string | null | undefined) {
  if (booking.value && bookingId === booking.value.id) load();
}
useLiveEvent('care', (event) => reloadIfShown(event.bookingId));
useLiveEvent('notification', (notification) => reloadIfShown(notification.bookingId));

onMounted(load);
</script>

<template>
  <div class="max-w-5xl">
    <button class="flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-700 mb-4" @click="router.back()">
      <ArrowLeft class="w-4 h-4" /> กลับ
    </button>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>

    <div v-else-if="booking" class="space-y-5">
      <!-- Header -->
      <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div>
            <p class="text-xs text-slate-400 font-mono">{{ booking.bookingCode }}</p>
            <h1 class="text-lg font-bold text-slate-800 mt-0.5 flex items-center gap-2">
              <CalendarDays class="w-5 h-5 text-slate-400" />
              {{ formatDateRange(booking.startDate, booking.endDate) }}
            </h1>
            <p class="text-sm text-slate-500 mt-0.5">{{ booking.daysCount }} วัน · แมว {{ booking.cats.length }} ตัว</p>
          </div>
          <StatusBadge :status="booking.status" />
        </div>

        <div v-if="other" class="flex flex-wrap items-center gap-3 mt-4 pt-4 border-t border-slate-100">
          <UserAvatar :name="other.name" :src="other.avatarUrl" size-class="w-11 h-11 text-base" />
          <div class="flex-1 min-w-0">
            <p class="text-xs text-slate-400">{{ isSitter ? 'เจ้าของแมว' : 'ผู้รับฝากเลี้ยง' }}</p>
            <p class="text-sm font-semibold text-slate-800 truncate">{{ other.name }}</p>
          </div>
          <a
            v-if="other.phone"
            :href="`tel:${other.phone}`"
            class="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-slate-200 text-sm text-slate-600 hover:bg-slate-50"
          >
            <Phone class="w-4 h-4" /> โทร
          </a>
          <button
            class="flex items-center gap-1.5 px-3 py-2 rounded-xl bg-slate-100 text-sm font-semibold text-slate-700 hover:bg-slate-200"
            @click="openChat"
          >
            <MessageCircle class="w-4 h-4" /> แชท
          </button>
        </div>

        <!-- Actions -->
        <p v-if="error" class="text-xs text-red-500 mt-3">{{ error }}</p>
        <div class="flex flex-wrap gap-2 mt-4 empty:hidden">
          <template v-if="isSitter && booking.status === 'pending'">
            <button :disabled="isBusy" class="px-4 py-2.5 rounded-xl bg-red-50 text-red-600 text-sm font-semibold disabled:opacity-50" @click="updateStatus('rejected', 'ต้องการปฏิเสธคำขอนี้ใช่หรือไม่?')">ปฏิเสธ</button>
            <button :disabled="isBusy" class="px-4 py-2.5 rounded-xl bg-[#FF7A59] text-white text-sm font-semibold disabled:opacity-50" @click="updateStatus('confirmed')">ตอบรับงาน</button>
          </template>
          <button
            v-if="isSitter && booking.status === 'confirmed'"
            :disabled="isBusy"
            class="px-4 py-2.5 rounded-xl bg-[#1E2238] text-white text-sm font-semibold disabled:opacity-50"
            @click="updateStatus('in_progress')"
          >
            เริ่มดูแล
          </button>
          <button
            v-if="isSitter && booking.status === 'in_progress'"
            :disabled="isBusy"
            class="px-4 py-2.5 rounded-xl bg-green-600 text-white text-sm font-semibold disabled:opacity-50"
            @click="updateStatus('completed', 'ยืนยันว่างานดูแลครั้งนี้เสร็จสิ้นแล้ว?')"
          >
            เสร็จสิ้นงาน
          </button>
          <button
            v-if="!isSitter && booking.status === 'pending'"
            :disabled="isBusy"
            class="px-4 py-2.5 rounded-xl bg-red-50 text-red-600 text-sm font-semibold disabled:opacity-50"
            @click="updateStatus('cancelled', 'ต้องการยกเลิกการจองนี้ใช่หรือไม่?')"
          >
            ยกเลิกการจอง
          </button>
        </div>
        <p v-if="booking.cancellationReason" class="text-sm text-red-500 bg-red-50 rounded-xl p-3 mt-4">
          เหตุผลที่ยกเลิก: {{ booking.cancellationReason }}
        </p>
      </div>

      <div class="grid lg:grid-cols-[1fr_360px] gap-5 items-start">
        <div class="space-y-5 min-w-0">
          <!-- Review -->
          <section
            v-if="booking.status === 'completed' && (booking.review || !isSitter)"
            class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5"
          >
            <h2 class="font-semibold text-slate-800 flex items-center gap-2 mb-3">
              <Star class="w-5 h-5 text-[#FFC857]" /> {{ isSitter ? 'รีวิวจากเจ้าของแมว' : 'รีวิวการดูแล' }}
            </h2>
            <div v-if="booking.review">
              <div class="flex items-center gap-2">
                <RatingStars :rating="booking.review.rating" :size="18" />
                <span class="text-xs text-slate-400">{{ formatRelativeTime(booking.review.createdAt) }}</span>
              </div>
              <p v-if="booking.review.comment" class="text-sm text-slate-600 mt-2 leading-relaxed">{{ booking.review.comment }}</p>
              <PhotoStrip :photos="booking.review.photos" alt="รูปรีวิว" class="mt-3" />
            </div>
            <ReviewForm v-else :booking-id="booking.id" :sitter-name="booking.sitter?.name" @submitted="onReviewed" />
          </section>

          <CarePanel v-if="showCare && care" :booking="booking" :care="care" :is-sitter="isSitter" @changed="onCareChanged" />
        </div>

        <div class="space-y-5">
          <!-- Cats -->
          <section class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
            <h2 class="font-semibold text-slate-800 flex items-center gap-2 mb-3"><CatIcon class="w-5 h-5 text-[#FF7A59]" /> น้องแมวที่ฝากดูแล</h2>
            <div class="space-y-3">
              <div v-for="cat in booking.cats" :key="cat.id" class="flex gap-3">
                <div class="w-12 h-12 rounded-xl bg-[#FFF0EB] overflow-hidden flex items-center justify-center shrink-0">
                  <img v-if="cat.photoUrl" :src="cat.photoUrl" class="w-full h-full object-cover" />
                  <CatIcon v-else class="w-6 h-6 text-[#FF7A59]" />
                </div>
                <div class="min-w-0 text-sm">
                  <p class="font-semibold text-slate-800">{{ cat.name }}</p>
                  <p class="text-xs text-slate-500">
                    {{ cat.breed ?? 'ไม่ระบุสายพันธุ์' }} · {{ ageDisplay(cat.ageYears, cat.ageMonths) }} · {{ cat.gender === 'female' ? 'เพศเมีย' : 'เพศผู้' }}
                  </p>
                  <p v-if="cat.dietaryHabits" class="text-xs text-slate-600 mt-1 flex gap-1"><Utensils class="w-3.5 h-3.5 shrink-0 mt-0.5 text-slate-400" /> {{ cat.dietaryHabits }}</p>
                  <p v-if="cat.medicalNotes" class="text-xs text-red-600 mt-1 flex gap-1"><HeartPulse class="w-3.5 h-3.5 shrink-0 mt-0.5" /> {{ cat.medicalNotes }}</p>
                </div>
              </div>
            </div>
            <div v-if="booking.specialInstructions" class="text-sm text-slate-600 bg-slate-50 rounded-xl p-3 mt-4">
              <p class="text-xs font-semibold text-slate-500 mb-0.5">คำแนะนำจากเจ้าของ</p>
              {{ booking.specialInstructions }}
            </div>
          </section>

          <!-- Price -->
          <section class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-sm space-y-2">
            <h2 class="font-semibold text-slate-800 mb-2">สรุปค่าบริการ</h2>
            <div class="flex justify-between text-slate-600"><span>ค่าบริการ ({{ booking.daysCount }} วัน)</span><span>{{ formatCurrency(booking.basePrice) }}</span></div>
            <div class="flex justify-between text-slate-600"><span>ค่าธรรมเนียมแพลตฟอร์ม</span><span>{{ formatCurrency(booking.serviceFee) }}</span></div>
            <div class="flex justify-between font-bold text-slate-800 pt-2 border-t border-slate-100">
              <span>ยอดรวม</span><span class="text-[#FF7A59] text-base">{{ formatCurrency(booking.totalPrice) }}</span>
            </div>
            <div v-if="isSitter" class="flex justify-between text-green-700 bg-green-50 rounded-lg px-3 py-2 font-semibold">
              <span>รายได้ของคุณ</span><span>{{ formatCurrency(booking.basePrice) }}</span>
            </div>
          </section>

          <!-- Payment Section -->
          <section class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 space-y-3.5">
            <div class="flex items-center justify-between">
              <h2 class="font-semibold text-slate-800 flex items-center gap-2">
                <CreditCard class="w-5 h-5 text-[#FF7A59]" />
                สถานะการชำระเงิน
              </h2>
              <StatusBadge :status="booking.paymentStatus" />
            </div>

            <!-- Case: Paid -->
            <div v-if="booking.paymentStatus === 'paid'" class="bg-emerald-50/70 border border-emerald-200/80 rounded-xl p-3.5 flex items-start gap-3">
              <CheckCircle2 class="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
              <div class="text-xs text-emerald-800 space-y-1">
                <p class="font-semibold text-emerald-900">ชำระเงินเรียบร้อยแล้ว</p>
                <p class="text-emerald-700">การชำระเงินได้รับการตรวจสอบและยืนยันแล้ว ระบบจะส่งรายได้ให้พี่เลี้ยงเมื่อดูแลเสร็จสิ้น</p>
                <button
                  v-if="booking.paymentSlipUrl"
                  type="button"
                  @click="showSlipModal = true"
                  class="text-emerald-800 underline font-semibold hover:text-emerald-950 inline-flex items-center gap-1 mt-1 cursor-pointer"
                >
                  <Eye class="w-3.5 h-3.5" /> ดูสลิปโอนเงิน
                </button>
              </div>
            </div>

            <!-- Case: Pending Verification -->
            <div v-else-if="booking.paymentStatus === 'pending_verification'" class="bg-amber-50/80 border border-amber-200/80 rounded-xl p-3.5 flex items-start gap-3">
              <Clock class="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
              <div class="text-xs text-amber-900 space-y-1">
                <p class="font-semibold text-amber-950">รอเจ้าหน้าที่ตรวจสอบสลิป</p>
                <p class="text-amber-700">
                  แนบสลิปแล้วเมื่อ {{ booking.paymentSlipUploadedAt ? formatRelativeTime(booking.paymentSlipUploadedAt) : 'เมื่อสักครู่' }}
                  เจ้าหน้าที่จะตรวจสอบยอดเงินให้โดยเร็วที่สุด
                </p>
                <div class="flex items-center gap-3 pt-1">
                  <button
                    v-if="booking.paymentSlipUrl"
                    type="button"
                    @click="showSlipModal = true"
                    class="text-amber-800 underline font-semibold hover:text-amber-950 inline-flex items-center gap-1 cursor-pointer"
                  >
                    <Eye class="w-3.5 h-3.5" /> ดูสลิปที่แนบ
                  </button>
                  <button
                    v-if="!isSitter"
                    type="button"
                    @click="openPaymentModal"
                    class="text-amber-800 underline font-semibold hover:text-amber-950 inline-flex items-center gap-1 cursor-pointer"
                  >
                    แนบสลิปใหม่
                  </button>
                </div>
              </div>
            </div>

            <!-- Case: Rejected -->
            <div v-else-if="booking.paymentStatus === 'rejected'" class="bg-red-50 border border-red-200 rounded-xl p-3.5 space-y-2.5">
              <div class="flex items-start gap-2.5">
                <AlertCircle class="w-5 h-5 text-red-600 shrink-0 mt-0.5" />
                <div class="text-xs text-red-800 flex-1">
                  <p class="font-semibold text-red-900">สลิปไม่ผ่านการตรวจสอบ</p>
                  <p v-if="booking.paymentRejectionReason" class="mt-1 bg-red-100/70 p-2 rounded-lg text-red-950 font-medium">
                    เหตุผล: {{ booking.paymentRejectionReason }}
                  </p>
                  <p v-else class="text-red-600 mt-0.5">กรุณาตรวจสอบยอดเงินและแนบสลิปใหม่อีกครั้ง</p>
                </div>
              </div>
              <div class="flex items-center justify-between pt-1">
                <button
                  v-if="booking.paymentSlipUrl"
                  type="button"
                  @click="showSlipModal = true"
                  class="text-xs text-red-700 underline font-semibold hover:text-red-950 inline-flex items-center gap-1 cursor-pointer"
                >
                  <Eye class="w-3.5 h-3.5" /> ดูสลิปเดิม
                </button>
                <button
                  v-if="!isSitter"
                  type="button"
                  @click="openPaymentModal"
                  class="px-3 py-1.5 bg-red-600 hover:bg-red-700 text-white text-xs font-semibold rounded-lg shadow-sm cursor-pointer ml-auto"
                >
                  แนบสลิปใหม่
                </button>
              </div>
            </div>

            <!-- Case: Unpaid -->
            <div v-else class="space-y-3">
              <div class="bg-slate-50 border border-slate-200 rounded-xl p-3.5 flex items-start gap-3">
                <CreditCard class="w-5 h-5 text-slate-500 shrink-0 mt-0.5" />
                <div class="text-xs text-slate-600">
                  <p class="font-semibold text-slate-800">ยังไม่ได้ชำระเงิน</p>
                  <p class="mt-0.5">กรุณาโอนเงินผ่าน PromptPay QR หรือบัญชีธนาคาร และแนบสลิป</p>
                </div>
              </div>

              <button
                v-if="!isSitter && booking.status !== 'cancelled' && booking.status !== 'rejected'"
                type="button"
                id="btn-open-payment"
                @click="openPaymentModal"
                class="w-full py-2.5 px-4 rounded-xl bg-gradient-to-r from-[#FF7A59] to-[#FF5E36] hover:from-[#f06f4e] hover:to-[#e64e26] text-white text-sm font-semibold shadow-md shadow-orange-500/15 flex items-center justify-center gap-2 transition cursor-pointer"
              >
                <CreditCard class="w-4 h-4" /> ชำระเงิน / แนบสลิปโอนเงิน
              </button>
            </div>
          </section>
        </div>
      </div>
    </div>

    <!-- Modal: Payment QR & Slip Upload -->
    <div
      v-if="showPaymentModal && booking"
      class="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4 overflow-y-auto"
    >
      <div class="bg-white rounded-2xl max-w-lg w-full shadow-2xl border border-slate-100 overflow-hidden my-6">
        <!-- Modal Header -->
        <div class="px-6 py-4 bg-slate-900 text-white flex items-center justify-between">
          <div>
            <h3 class="font-bold text-base flex items-center gap-2">
              <CreditCard class="w-5 h-5 text-[#FF7A59]" />
              ชำระเงินและแนบสลิป
            </h3>
            <p class="text-xs text-slate-300 mt-0.5 font-mono">รหัสการจอง: {{ booking.bookingCode }}</p>
          </div>
          <button
            type="button"
            @click="showPaymentModal = false"
            class="text-slate-400 hover:text-white p-1 rounded-lg hover:bg-slate-800 transition cursor-pointer"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <div class="p-6 space-y-5 max-h-[80vh] overflow-y-auto">
          <!-- Amount Highlight -->
          <div class="bg-gradient-to-br from-orange-50 to-amber-50 border border-orange-200/60 rounded-xl p-4 text-center">
            <p class="text-xs text-slate-500 font-medium">ยอดเงินที่ต้องชำระ</p>
            <p class="text-2xl font-black text-[#FF7A59] mt-0.5">{{ formatCurrency(booking.totalPrice) }}</p>
            <p class="text-xs text-slate-400 mt-0.5">รวมค่าบริการและค่าธรรมเนียมแพลตฟอร์มแล้ว</p>
          </div>

          <!-- PromptPay QR Code Box -->
          <div class="border border-slate-200 rounded-2xl overflow-hidden bg-white shadow-xs">
            <!-- Official Thai PromptPay Header -->
            <div class="bg-[#003D6A] text-white px-4 py-2.5 text-center flex items-center justify-center gap-2">
              <span class="font-black text-sm tracking-wide">PromptPay</span>
              <span class="text-xs text-blue-200 font-normal">พร้อมเพย์</span>
            </div>

            <div class="p-4 flex flex-col items-center">
              <div class="bg-white p-2 rounded-xl border border-slate-200 shadow-xs mb-3">
                <img
                  :src="`https://api.qrserver.com/v1/create-qr-code/?size=180x180&data=PROMPTPAY:0105566000000:AMOUNT:${booking.totalPrice}:REF:${booking.bookingCode}`"
                  alt="PromptPay QR Code"
                  class="w-44 h-44 object-contain rounded"
                />
              </div>
              <p class="text-xs font-semibold text-slate-700">สแกน QR Code ด้วย Mobile Banking ทุกธนาคาร</p>
              <p class="text-xs text-slate-400 mt-0.5">ชื่อบัญชี: บจก. แคท ซิตเตอร์ (ไทยแลนด์)</p>
            </div>
          </div>

          <!-- Bank Account Info -->
          <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-2 text-xs">
            <p class="font-semibold text-slate-800 text-sm">หรือโอนผ่านบัญชีธนาคาร</p>
            <div class="flex items-center justify-between text-slate-600">
              <span>ธนาคาร:</span>
              <span class="font-medium text-slate-800">กสิกรไทย (KBANK)</span>
            </div>
            <div class="flex items-center justify-between text-slate-600">
              <span>เลขที่บัญชี:</span>
              <div class="flex items-center gap-2">
                <span class="font-mono font-bold text-slate-900 text-sm">123-2-88990-0</span>
                <button
                  type="button"
                  @click="copyText('1232889900', 'acc')"
                  class="p-1 rounded bg-white border border-slate-200 text-slate-600 hover:text-slate-900 cursor-pointer"
                  title="คัดลอกเลขบัญชี"
                >
                  <Check v-if="copiedField === 'acc'" class="w-3.5 h-3.5 text-green-600" />
                  <Copy v-else class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
            <div class="flex items-center justify-between text-slate-600">
              <span>ชื่อบัญชี:</span>
              <span class="font-medium text-slate-800">บจก. แคท ซิตเตอร์ (ไทยแลนด์)</span>
            </div>
          </div>

          <!-- Upload Slip Section -->
          <div class="space-y-3">
            <div class="flex items-center justify-between">
              <label class="text-xs font-bold text-slate-700 uppercase tracking-wider">แนบหลักฐานการโอนเงิน (สลิป)</label>
              <button
                type="button"
                id="btn-use-demo-slip"
                @click="useDemoSlip"
                class="text-xs text-[#FF7A59] hover:underline font-semibold cursor-pointer"
              >
                + ใช้สลิปจำลองทดสอบ
              </button>
            </div>

            <!-- Upload Area -->
            <div
              v-if="!selectedSlipFile"
              class="border-2 border-dashed border-slate-300 hover:border-[#FF7A59] rounded-2xl p-6 text-center cursor-pointer transition bg-slate-50/50 hover:bg-orange-50/20"
              @click="($refs.fileInput as HTMLInputElement)?.click()"
            >
              <input
                ref="fileInput"
                type="file"
                accept="image/*"
                class="hidden"
                id="input-slip-file"
                @change="onSlipFileChange"
              />
              <Upload class="w-8 h-8 text-slate-400 mx-auto mb-2" />
              <p class="text-sm font-semibold text-slate-700">คลิกเพื่ออัปโหลดรูปภาพสลิป</p>
              <p class="text-xs text-slate-400 mt-1">รองรับ JPG, PNG, WebP ขนาดไม่เกิน 10MB</p>
            </div>

            <!-- Slip Preview -->
            <div v-else class="relative border border-slate-200 rounded-xl p-3 bg-slate-50 flex items-center gap-3">
              <img
                :src="slipPreviewUrl!"
                alt="Preview"
                class="w-16 h-20 object-cover rounded-lg border border-slate-200 bg-white"
              />
              <div class="flex-1 min-w-0 text-xs">
                <p class="font-semibold text-slate-800 truncate">{{ selectedSlipFile.name }}</p>
                <p class="text-slate-500 mt-0.5">{{ (selectedSlipFile.size / 1024).toFixed(1) }} KB</p>
                <span class="inline-block mt-1 text-[11px] text-green-600 bg-green-50 px-2 py-0.5 rounded font-medium">พร้อมอัปโหลด</span>
              </div>
              <button
                type="button"
                @click="removeSlipFile"
                class="p-1.5 rounded-lg text-slate-400 hover:text-red-500 hover:bg-red-50 transition cursor-pointer"
              >
                <X class="w-4 h-4" />
              </button>
            </div>

            <p v-if="slipError" class="text-xs text-red-600 font-medium bg-red-50 p-2.5 rounded-lg">{{ slipError }}</p>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex items-center justify-end gap-3">
          <button
            type="button"
            @click="showPaymentModal = false"
            class="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-200 rounded-xl transition cursor-pointer"
          >
            ยกเลิก
          </button>
          <button
            type="button"
            id="btn-submit-slip"
            :disabled="!selectedSlipFile || isUploadingSlip"
            @click="submitPaymentSlip"
            class="px-5 py-2.5 text-xs font-bold text-white bg-[#FF7A59] hover:bg-[#f06f4e] rounded-xl shadow-md shadow-orange-500/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 cursor-pointer transition"
          >
            <span v-if="isUploadingSlip" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>{{ isUploadingSlip ? 'กำลังส่งสลิป...' : 'ยืนยันและส่งสลิปโอนเงิน' }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: View Slip Image -->
    <div
      v-if="showSlipModal && booking?.paymentSlipUrl"
      class="fixed inset-0 bg-slate-950/80 backdrop-blur-xs z-50 flex items-center justify-center p-4"
      @click="showSlipModal = false"
    >
      <div
        class="bg-white rounded-2xl max-w-md w-full shadow-2xl overflow-hidden border border-slate-100"
        @click.stop
      >
        <div class="px-5 py-3.5 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h4 class="font-bold text-slate-800 text-sm">หลักฐานการโอนเงิน (สลิป)</h4>
            <p class="text-xs text-slate-400 font-mono">{{ booking.bookingCode }}</p>
          </div>
          <button
            type="button"
            @click="showSlipModal = false"
            class="p-1 rounded-lg text-slate-400 hover:text-slate-700 hover:bg-slate-100 cursor-pointer"
          >
            <X class="w-5 h-5" />
          </button>
        </div>
        <div class="p-4 bg-slate-50 flex items-center justify-center max-h-[75vh] overflow-auto">
          <img
            :src="fullSlipUrl(booking.paymentSlipUrl)"
            alt="Payment Slip"
            class="max-w-full max-h-[65vh] object-contain rounded-lg shadow-sm"
          />
        </div>
        <div class="px-5 py-3 bg-white border-t border-slate-100 text-center">
          <button
            type="button"
            @click="showSlipModal = false"
            class="px-5 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold rounded-xl cursor-pointer"
          >
            ปิดหน้าต่าง
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
