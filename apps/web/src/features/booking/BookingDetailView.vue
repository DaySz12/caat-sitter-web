<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, MessageCircle, Phone, CalendarDays, Cat as CatIcon, HeartPulse, Utensils, Star } from 'lucide-vue-next';
import { bookingApi } from './booking.api';
import { careApi } from '@/features/care/care.api';
import { chatApi } from '@/features/chat/chat.api';
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

      <div class="grid lg:grid-cols-[1fr_340px] gap-5 items-start">
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
            <div class="flex justify-between text-slate-600"><span>ค่าบริการ ({{ booking.daysCount }} วัน)</span><span>{{ formatCurrency(booking.basePrice) }}</span></div>
            <div class="flex justify-between text-slate-600"><span>ค่าธรรมเนียมแพลตฟอร์ม</span><span>{{ formatCurrency(booking.serviceFee) }}</span></div>
            <div class="flex justify-between font-bold text-slate-800 pt-2 border-t border-slate-100">
              <span>ยอดรวม</span><span class="text-[#FF7A59]">{{ formatCurrency(booking.totalPrice) }}</span>
            </div>
            <div v-if="isSitter" class="flex justify-between text-green-700 bg-green-50 rounded-lg px-3 py-2 font-semibold">
              <span>รายได้ของคุณ</span><span>{{ formatCurrency(booking.basePrice) }}</span>
            </div>
          </section>
        </div>
      </div>
    </div>
  </div>
</template>
