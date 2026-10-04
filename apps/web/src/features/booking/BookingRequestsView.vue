<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { bookingApi } from './booking.api';
import BookingCard from './BookingCard.vue';
import type { BookingDto } from '@/shared/types/models';

const bookings = ref<BookingDto[]>([]);
const isLoading = ref(true);
const busyId = ref<string | null>(null);

async function load() {
  isLoading.value = true;
  bookings.value = await bookingApi.mine('pending');
  isLoading.value = false;
}

async function respond(booking: BookingDto, accept: boolean) {
  busyId.value = booking.id;
  try {
    await bookingApi.updateStatus(booking.id, accept ? 'confirmed' : 'rejected');
    await load();
  } finally {
    busyId.value = null;
  }
}

onMounted(load);
</script>

<template>
  <div>
    <h1 class="text-xl font-bold text-slate-800 mb-1">คำขอจองใหม่</h1>
    <p class="text-sm text-slate-500 mb-5">ตอบรับหรือปฏิเสธคำขอจากเจ้าของแมว</p>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>
    <div v-else-if="bookings.length === 0" class="text-center py-16 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
      ยังไม่มีคำขอจองใหม่
    </div>
    <div v-else class="space-y-3">
      <BookingCard v-for="booking in bookings" :key="booking.id" :booking="booking" perspective="sitter">
        <template #actions="{ booking: b }">
          <div class="flex gap-2">
            <button
              :disabled="busyId === b.id"
              class="px-3 py-1.5 rounded-lg bg-red-50 text-red-600 text-xs font-semibold disabled:opacity-50"
              @click="respond(b, false)"
            >
              ปฏิเสธ
            </button>
            <button
              :disabled="busyId === b.id"
              class="px-3 py-1.5 rounded-lg bg-[#FF7A59] text-white text-xs font-semibold disabled:opacity-50"
              @click="respond(b, true)"
            >
              ตอบรับ
            </button>
          </div>
        </template>
      </BookingCard>
    </div>
  </div>
</template>
