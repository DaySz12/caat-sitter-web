<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { Cat as CatIcon, Search, CalendarCheck, ArrowRight, Activity, CheckCircle2, Wallet, Star, ClipboardCheck } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { useSummaryStore } from '@/features/notifications/summary.store';
import { catsApi } from '@/features/cats/cats.api';
import { bookingApi } from '@/features/booking/booking.api';
import { bannersApi } from '@/shared/lib/banners.api';
import BookingCard from '@/features/booking/BookingCard.vue';
import BannerCarousel from '@/shared/ui/BannerCarousel.vue';
import PopupModal from '@/shared/ui/PopupModal.vue';
import type { BookingDto, CatDto, BannerDto } from '@/shared/types/models';
import { formatCurrency } from '@/shared/lib/format';
import StatTile from '@/shared/ui/StatTile.vue';

const auth = useAuthStore();
const summaryStore = useSummaryStore();
const router = useRouter();
const cats = ref<CatDto[]>([]);
const bookings = ref<BookingDto[]>([]);
const banners = ref<BannerDto[]>([]);
const popups = ref<BannerDto[]>([]);

const activeBookings = computed(() =>
  bookings.value.filter((b) => b.status === 'pending' || b.status === 'confirmed' || b.status === 'in_progress').slice(0, 3),
);
const inCare = computed(() => bookings.value.filter((b) => b.status === 'in_progress'));
const awaitingReview = computed(() => bookings.value.filter((b) => b.status === 'completed' && !b.review));

onMounted(async () => {
  const [catsData, bookingsData, bannersData] = await Promise.all([
    catsApi.mine(),
    bookingApi.mine(),
    bannersApi.list().catch(() => []),
  ]);
  cats.value = catsData;
  bookings.value = bookingsData;
  banners.value = (bannersData || []).filter((b) => b.type === 'banner');
  popups.value = (bannersData || []).filter((b) => b.type === 'popup');
});
</script>

<template>
  <div>
    <!-- Popup Modal -->
    <PopupModal :popups="popups" />

    <!-- Top Banner Carousel -->
    <BannerCarousel v-if="banners.length > 0" :banners="banners" />

    <div class="bg-gradient-to-r from-[#FF7A59] to-[#FF9E85] rounded-2xl p-6 text-white mb-6">
      <h1 class="text-xl font-bold">สวัสดี, {{ auth.user?.name }} 🐾</h1>
      <p class="text-sm opacity-90 mt-1">คุณมีแมว {{ cats.length }} ตัว และการจองที่กำลังดำเนินการ {{ activeBookings.length }} รายการ</p>
    </div>

    <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 mb-6">
      <StatTile :icon="CatIcon" label="แมวของฉัน" :value="`${cats.length} ตัว`" icon-class="bg-[#FFF0EB] text-[#FF7A59]" />
      <StatTile :icon="Activity" label="การจองที่ยังดำเนินอยู่" :value="summaryStore.summary?.activeBookings ?? '–'" icon-class="bg-blue-50 text-blue-600" />
      <StatTile :icon="CheckCircle2" label="ฝากเลี้ยงสำเร็จ" :value="`${summaryStore.summary?.completedBookings ?? '–'} ครั้ง`" icon-class="bg-green-50 text-green-600" />
      <StatTile :icon="Wallet" label="ค่าใช้จ่ายทั้งหมด" :value="formatCurrency(summaryStore.summary?.totalSpent ?? 0)" icon-class="bg-amber-50 text-amber-600" />
    </div>

    <!-- Live care: the most reassuring thing an owner can see -->
    <RouterLink
      v-for="b in inCare"
      :key="b.id"
      :to="`/owner/bookings/${b.id}`"
      class="flex items-center gap-4 bg-white rounded-2xl border border-green-100 shadow-sm p-4 mb-4 hover:shadow-md transition"
    >
      <div class="w-11 h-11 rounded-xl bg-green-50 text-green-600 flex items-center justify-center shrink-0">
        <ClipboardCheck class="w-6 h-6" />
      </div>
      <div class="flex-1 min-w-0">
        <p class="text-sm font-semibold text-slate-800 flex items-center gap-2">
          <span class="w-2 h-2 rounded-full bg-green-500 animate-pulse" />
          {{ b.sitter?.name }} กำลังดูแล {{ b.cats.map((c) => c.name).join(', ') }}
        </p>
        <p class="text-xs text-slate-500 mt-0.5">ดูเช็กลิสต์ประจำวันและเวลาเช็กอินของพี่เลี้ยง</p>
      </div>
      <ArrowRight class="w-4 h-4 text-slate-400 shrink-0" />
    </RouterLink>

    <RouterLink
      v-if="awaitingReview.length"
      :to="`/owner/bookings/${awaitingReview[0].id}`"
      class="flex items-center gap-4 bg-amber-50 rounded-2xl border border-amber-100 p-4 mb-6 hover:bg-amber-100/60 transition"
    >
      <Star class="w-6 h-6 text-[#FFC857] fill-[#FFC857] shrink-0" />
      <div class="flex-1 min-w-0">
        <p class="text-sm font-semibold text-slate-800">ให้คะแนน {{ awaitingReview[0].sitter?.name }}</p>
        <p class="text-xs text-slate-500 mt-0.5">
          รีวิวของคุณช่วยให้เจ้าของแมวคนอื่นเลือกพี่เลี้ยงได้ง่ายขึ้น
          <template v-if="awaitingReview.length > 1">(รอรีวิว {{ awaitingReview.length }} รายการ)</template>
        </p>
      </div>
      <ArrowRight class="w-4 h-4 text-slate-400 shrink-0" />
    </RouterLink>

    <div class="grid sm:grid-cols-3 gap-4 mb-6">
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/owner/cats')">
        <CatIcon class="w-6 h-6 text-[#FF7A59] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">แมวของฉัน</p>
        <p class="text-xs text-slate-500 mt-0.5">จัดการข้อมูลแมว</p>
      </button>
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/owner/sitters')">
        <Search class="w-6 h-6 text-[#FF7A59] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">ค้นหาผู้รับฝากเลี้ยง</p>
        <p class="text-xs text-slate-500 mt-0.5">Smart Matching ตามระยะทาง</p>
      </button>
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/owner/bookings')">
        <CalendarCheck class="w-6 h-6 text-[#FF7A59] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">ประวัติการจอง</p>
        <p class="text-xs text-slate-500 mt-0.5">ติดตามสถานะการจอง</p>
      </button>
    </div>

    <div>
      <div class="flex items-center justify-between mb-3">
        <h2 class="font-semibold text-slate-800">การจองล่าสุด</h2>
        <button class="flex items-center gap-1 text-xs text-[#FF7A59] font-semibold" @click="router.push('/owner/bookings')">
          ดูทั้งหมด <ArrowRight class="w-3 h-3" />
        </button>
      </div>
      <div v-if="activeBookings.length === 0" class="text-center py-10 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
        ยังไม่มีการจองที่กำลังดำเนินการ
      </div>
      <div v-else class="space-y-3">
        <BookingCard v-for="b in activeBookings" :key="b.id" :booking="b" perspective="owner" />
      </div>
    </div>
  </div>
</template>
