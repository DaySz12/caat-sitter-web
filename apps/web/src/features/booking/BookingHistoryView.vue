<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { bookingApi } from './booking.api';
import BookingCard from './BookingCard.vue';
import type { BookingDto, BookingStatus } from '@/shared/types/models';

const bookings = ref<BookingDto[]>([]);
const isLoading = ref(true);
const activeTab = ref<BookingStatus | 'all'>('all');

const tabs: { value: BookingStatus | 'all'; label: string }[] = [
  { value: 'all', label: 'ทั้งหมด' },
  { value: 'pending', label: 'รอดำเนินการ' },
  { value: 'confirmed', label: 'ยืนยันแล้ว' },
  { value: 'in_progress', label: 'กำลังดำเนินการ' },
  { value: 'completed', label: 'เสร็จสิ้น' },
  { value: 'cancelled', label: 'ยกเลิก' },
];

async function load() {
  isLoading.value = true;
  bookings.value = await bookingApi.mine(activeTab.value);
  isLoading.value = false;
}

async function cancelBooking(booking: BookingDto) {
  if (!confirm('ต้องการยกเลิกการจองนี้ใช่หรือไม่?')) return;
  await bookingApi.updateStatus(booking.id, 'cancelled');
  await load();
}

onMounted(load);
watch(activeTab, load);
</script>

<template>
  <div>
    <h1 class="text-xl font-bold text-slate-800 mb-1">ประวัติการจอง</h1>
    <p class="text-sm text-slate-500 mb-5">ติดตามสถานะการจองผู้รับฝากเลี้ยงแมวของคุณ</p>

    <div class="flex gap-1.5 overflow-x-auto pb-2 mb-4">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        class="px-3.5 py-1.5 rounded-full text-xs font-medium whitespace-nowrap border transition"
        :class="activeTab === tab.value ? 'bg-[#FF7A59] text-white border-[#FF7A59]' : 'bg-white text-slate-500 border-slate-200'"
        @click="activeTab = tab.value"
      >
        {{ tab.label }}
      </button>
    </div>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>
    <div v-else-if="bookings.length === 0" class="text-center py-16 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
      ยังไม่มีรายการจองในหมวดนี้
    </div>
    <div v-else class="space-y-3">
      <BookingCard v-for="booking in bookings" :key="booking.id" :booking="booking" perspective="owner">
        <template #actions="{ booking: b }">
          <button
            v-if="b.status === 'pending'"
            class="text-xs font-semibold text-red-500 hover:underline"
            @click="cancelBooking(b)"
          >
            ยกเลิก
          </button>
        </template>
      </BookingCard>
    </div>
  </div>
</template>
