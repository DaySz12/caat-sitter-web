<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Cat as CatIcon, Search, CalendarCheck, ArrowRight } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { catsApi } from '@/features/cats/cats.api';
import { bookingApi } from '@/features/booking/booking.api';
import BookingCard from '@/features/booking/BookingCard.vue';
import type { BookingDto, CatDto } from '@/shared/types/models';

const auth = useAuthStore();
const router = useRouter();
const cats = ref<CatDto[]>([]);
const activeBookings = ref<BookingDto[]>([]);

onMounted(async () => {
  const [catsData, bookingsData] = await Promise.all([catsApi.mine(), bookingApi.mine()]);
  cats.value = catsData;
  activeBookings.value = bookingsData.filter((b) => b.status === 'pending' || b.status === 'confirmed' || b.status === 'in_progress').slice(0, 3);
});
</script>

<template>
  <div>
    <div class="bg-gradient-to-r from-[#FF7A59] to-[#FF9E85] rounded-2xl p-6 text-white mb-6">
      <h1 class="text-xl font-bold">สวัสดี, {{ auth.user?.name }} 🐾</h1>
      <p class="text-sm opacity-90 mt-1">คุณมีแมว {{ cats.length }} ตัว และการจองที่กำลังดำเนินการ {{ activeBookings.length }} รายการ</p>
    </div>

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
