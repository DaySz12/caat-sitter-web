<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { bookingApi } from './booking.api';
import BookingCard from './BookingCard.vue';
import type { BookingDto } from '@/shared/types/models';

const bookings = ref<BookingDto[]>([]);
const isLoading = ref(true);
const busyId = ref<string | null>(null);
const activeTab = ref<'confirmed' | 'in_progress' | 'completed'>('confirmed');

const tabs = [
  { value: 'confirmed' as const, label: 'รอเริ่มงาน' },
  { value: 'in_progress' as const, label: 'กำลังดูแล' },
  { value: 'completed' as const, label: 'เสร็จสิ้น' },
];

async function load() {
  isLoading.value = true;
  bookings.value = await bookingApi.mine(activeTab.value);
  isLoading.value = false;
}

async function advance(booking: BookingDto, nextStatus: 'in_progress' | 'completed') {
  busyId.value = booking.id;
  try {
    await bookingApi.updateStatus(booking.id, nextStatus);
    await load();
  } finally {
    busyId.value = null;
  }
}

onMounted(load);
watch(activeTab, load);
</script>

<template>
  <div>
    <h1 class="text-xl font-bold text-slate-800 mb-1">งานของฉัน</h1>
    <p class="text-sm text-slate-500 mb-5">รายการงานที่ตอบรับแล้ว</p>

    <div class="flex gap-1.5 mb-4">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        class="px-3.5 py-1.5 rounded-full text-xs font-medium border transition"
        :class="activeTab === tab.value ? 'bg-[#1E2238] text-white border-[#1E2238]' : 'bg-white text-slate-500 border-slate-200'"
        @click="activeTab = tab.value"
      >
        {{ tab.label }}
      </button>
    </div>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>
    <div v-else-if="bookings.length === 0" class="text-center py-16 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
      ยังไม่มีงานในหมวดนี้
    </div>
    <div v-else class="space-y-3">
      <BookingCard v-for="booking in bookings" :key="booking.id" :booking="booking" perspective="sitter">
        <template #actions="{ booking: b }">
          <button
            v-if="b.status === 'confirmed'"
            :disabled="busyId === b.id"
            class="px-3 py-1.5 rounded-lg bg-[#1E2238] text-white text-xs font-semibold disabled:opacity-50"
            @click="advance(b, 'in_progress')"
          >
            เริ่มดูแล
          </button>
          <button
            v-else-if="b.status === 'in_progress'"
            :disabled="busyId === b.id"
            class="px-3 py-1.5 rounded-lg bg-green-600 text-white text-xs font-semibold disabled:opacity-50"
            @click="advance(b, 'completed')"
          >
            เสร็จสิ้นงาน
          </button>
        </template>
      </BookingCard>
    </div>
  </div>
</template>
