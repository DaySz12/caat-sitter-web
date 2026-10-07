<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ClipboardList, Briefcase, ArrowRight, ShieldAlert, Wallet, Star } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { useSummaryStore } from '@/features/notifications/summary.store';
import { bookingApi } from '@/features/booking/booking.api';
import { sittersApi } from '@/features/sitters/sitters.api';
import { reviewsApi } from '@/features/reviews/reviews.api';
import BookingCard from '@/features/booking/BookingCard.vue';
import ReviewList from '@/features/reviews/ReviewList.vue';
import type { BookingDto, ReviewDto, SitterDto } from '@/shared/types/models';
import { formatCurrency } from '@/shared/lib/format';
import StatTile from '@/shared/ui/StatTile.vue';

const auth = useAuthStore();
const summaryStore = useSummaryStore();
const router = useRouter();
const pendingRequests = ref<BookingDto[]>([]);
const activeJobs = ref<BookingDto[]>([]);
const profile = ref<SitterDto | null>(null);
const reviews = ref<ReviewDto[]>([]);

onMounted(async () => {
  const sitterId = auth.user!.id;
  const [pending, confirmed, inProgress, sitterProfile, sitterReviews] = await Promise.all([
    bookingApi.mine('pending'),
    bookingApi.mine('confirmed'),
    bookingApi.mine('in_progress'),
    sittersApi.getById(sitterId).catch(() => null),
    reviewsApi.forSitter(sitterId),
  ]);
  pendingRequests.value = pending;
  activeJobs.value = [...inProgress, ...confirmed].slice(0, 3);
  profile.value = sitterProfile;
  reviews.value = sitterReviews.slice(0, 3);
});
</script>

<template>
  <div>
    <div
      v-if="auth.user?.status === 'pending'"
      class="flex items-start gap-3 bg-amber-50 border border-amber-200 rounded-2xl p-4 mb-6 text-amber-700"
    >
      <ShieldAlert class="w-5 h-5 shrink-0 mt-0.5" />
      <div>
        <p class="font-semibold text-sm">บัญชีของคุณกำลังรอการตรวจสอบ</p>
        <p class="text-xs mt-0.5">ทีมงานจะตรวจสอบข้อมูลของคุณก่อนเปิดให้รับงานได้ กรุณารอการอนุมัติ</p>
      </div>
    </div>

    <div class="bg-gradient-to-r from-[#1E2238] to-[#2E3456] rounded-2xl p-6 text-white mb-6">
      <h1 class="text-xl font-bold">สวัสดี, {{ auth.user?.name }} 👋</h1>
      <p class="text-sm opacity-90 mt-1">คุณมีคำขอจองใหม่ {{ pendingRequests.length }} รายการรออนุมัติ</p>
    </div>

    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-6">
      <StatTile :icon="ClipboardList" label="คำขอจองใหม่" :value="pendingRequests.length" icon-class="bg-[#FFF0EB] text-[#FF7A59]" />
      <StatTile :icon="Briefcase" label="งานที่ต้องดูแล" :value="summaryStore.summary?.activeBookings ?? '–'" icon-class="bg-blue-50 text-blue-600" />
      <StatTile :icon="Wallet" label="รายได้เดือนนี้" :value="formatCurrency(summaryStore.summary?.monthEarnings ?? 0)" icon-class="bg-green-50 text-green-600" />
      <StatTile
        :icon="Star"
        label="คะแนนเฉลี่ย"
        :value="profile && profile.reviewCount ? `${profile.ratingAvg.toFixed(1)} (${profile.reviewCount})` : 'ยังไม่มีรีวิว'"
        icon-class="bg-amber-50 text-amber-500"
      />
    </div>

    <div class="grid sm:grid-cols-2 gap-4 mb-6">
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/sitter/requests')">
        <ClipboardList class="w-6 h-6 text-[#1E2238] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">คำขอจองใหม่</p>
        <p class="text-xs text-slate-500 mt-0.5">{{ pendingRequests.length }} รายการรอตอบรับ</p>
      </button>
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/sitter/jobs')">
        <Briefcase class="w-6 h-6 text-[#1E2238] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">งานของฉัน</p>
        <p class="text-xs text-slate-500 mt-0.5">งานที่ตอบรับแล้ว · รายได้รวม {{ formatCurrency(summaryStore.summary?.totalEarnings ?? 0) }}</p>
      </button>
    </div>

    <div class="grid lg:grid-cols-2 gap-6">
      <div>
        <div class="flex items-center justify-between mb-3">
          <h2 class="font-semibold text-slate-800">งานที่กำลังดำเนินการ</h2>
          <button class="flex items-center gap-1 text-xs text-[#1E2238] font-semibold" @click="router.push('/sitter/jobs')">
            ดูทั้งหมด <ArrowRight class="w-3 h-3" />
          </button>
        </div>
        <div v-if="activeJobs.length === 0" class="text-center py-10 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
          ยังไม่มีงานที่กำลังดำเนินการ
        </div>
        <div v-else class="space-y-3">
          <BookingCard v-for="b in activeJobs" :key="b.id" :booking="b" perspective="sitter" />
        </div>
      </div>

      <div>
        <h2 class="font-semibold text-slate-800 mb-3">รีวิวล่าสุด</h2>
        <div v-if="reviews.length === 0" class="text-center py-10 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
          ยังไม่มีรีวิว
        </div>
        <div v-else class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
          <ReviewList :reviews="reviews" />
        </div>
      </div>
    </div>
  </div>
</template>
