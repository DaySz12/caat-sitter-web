<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ClipboardList, Briefcase, ArrowRight, ShieldAlert } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { bookingApi } from '@/features/booking/booking.api';
import BookingCard from '@/features/booking/BookingCard.vue';
import type { BookingDto } from '@/shared/types/models';

const auth = useAuthStore();
const router = useRouter();
const pendingRequests = ref<BookingDto[]>([]);
const activeJobs = ref<BookingDto[]>([]);

onMounted(async () => {
  const [pending, confirmed, inProgress] = await Promise.all([
    bookingApi.mine('pending'),
    bookingApi.mine('confirmed'),
    bookingApi.mine('in_progress'),
  ]);
  pendingRequests.value = pending;
  activeJobs.value = [...confirmed, ...inProgress].slice(0, 3);
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

    <div class="grid sm:grid-cols-2 gap-4 mb-6">
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/sitter/requests')">
        <ClipboardList class="w-6 h-6 text-[#1E2238] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">คำขอจองใหม่</p>
        <p class="text-xs text-slate-500 mt-0.5">{{ pendingRequests.length }} รายการรอตอบรับ</p>
      </button>
      <button class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5 text-left hover:shadow-md transition" @click="router.push('/sitter/jobs')">
        <Briefcase class="w-6 h-6 text-[#1E2238] mb-2" />
        <p class="font-semibold text-slate-800 text-sm">งานของฉัน</p>
        <p class="text-xs text-slate-500 mt-0.5">งานที่ตอบรับแล้ว</p>
      </button>
    </div>

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
  </div>
</template>
