<script setup lang="ts">
import { Cat as CatIcon, User } from 'lucide-vue-next';
import type { BookingDto } from '@/shared/types/models';
import { formatCurrency, formatDateRange } from '@/shared/lib/format';
import StatusBadge from '@/shared/ui/StatusBadge.vue';

defineProps<{ booking: BookingDto; perspective: 'owner' | 'sitter' }>();
</script>

<template>
  <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-4">
    <div class="flex items-start justify-between mb-3">
      <div>
        <p class="text-xs text-slate-400 font-mono">{{ booking.bookingCode }}</p>
        <p class="text-sm font-semibold text-slate-800 mt-0.5 flex items-center gap-1.5">
          <User class="w-3.5 h-3.5 text-slate-400" />
          {{ perspective === 'owner' ? booking.sitter?.name : booking.owner?.name }}
        </p>
      </div>
      <StatusBadge :status="booking.status" />
    </div>

    <p class="text-sm text-slate-600">{{ formatDateRange(booking.startDate, booking.endDate) }} · {{ booking.daysCount }} วัน</p>

    <div class="flex flex-wrap gap-1.5 mt-2">
      <span v-for="cat in booking.cats" :key="cat.id" class="flex items-center gap-1 text-xs px-2 py-1 rounded-full bg-[#FFF0EB] text-[#E05638]">
        <CatIcon class="w-3 h-3" /> {{ cat.name }}
      </span>
    </div>

    <div v-if="booking.specialInstructions" class="text-xs text-slate-500 mt-2 bg-slate-50 rounded-lg p-2">
      {{ booking.specialInstructions }}
    </div>

    <div class="flex items-center justify-between mt-3 pt-3 border-t border-slate-100">
      <p class="font-bold text-[#FF7A59]">{{ formatCurrency(booking.totalPrice) }}</p>
      <slot name="actions" :booking="booking" />
    </div>
  </div>
</template>
