<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import { Cat as CatIcon, User, ChevronRight } from 'lucide-vue-next';
import type { BookingDto } from '@/shared/types/models';
import { formatCurrency, formatDateRange } from '@/shared/lib/format';
import { bookingDetailPath } from '@/shared/lib/paths';
import StatusBadge from '@/shared/ui/StatusBadge.vue';
import RatingStars from '@/shared/ui/RatingStars.vue';

const props = defineProps<{ booking: BookingDto; perspective: 'owner' | 'sitter' }>();

const detailPath = computed(() => bookingDetailPath(props.perspective, props.booking.id));
</script>

<template>
  <div class="bg-white rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition">
    <RouterLink :to="detailPath" class="block p-4 pb-0">
      <div class="flex items-start justify-between mb-3">
        <div>
          <p class="text-xs text-slate-400 font-mono">{{ booking.bookingCode }}</p>
          <p class="text-sm font-semibold text-slate-800 mt-0.5 flex items-center gap-1.5">
            <User class="w-3.5 h-3.5 text-slate-400" />
            {{ perspective === 'owner' ? booking.sitter?.name : booking.owner?.name }}
          </p>
        </div>
        <div class="flex items-center gap-1.5 flex-wrap justify-end">
          <StatusBadge :status="booking.status" />
          <StatusBadge :status="booking.paymentStatus" />
          <ChevronRight class="w-4 h-4 text-slate-300" />
        </div>
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
    </RouterLink>

    <div class="flex items-center justify-between gap-2 mx-4 mt-3 py-3 border-t border-slate-100">
      <div class="flex items-center gap-2">
        <p class="font-bold text-[#FF7A59]">{{ formatCurrency(booking.totalPrice) }}</p>
        <RatingStars v-if="booking.review" :rating="booking.review.rating" :size="12" />
      </div>
      <div class="flex items-center gap-2">
        <RouterLink
          v-if="perspective === 'owner' && booking.paymentStatus === 'unpaid' && booking.status !== 'cancelled' && booking.status !== 'rejected'"
          :to="detailPath"
          class="px-2.5 py-1 rounded-lg bg-orange-50 text-[#FF7A59] border border-orange-200 text-xs font-semibold hover:bg-orange-100 transition"
        >
          💳 ชำระเงิน
        </RouterLink>
        <slot name="actions" :booking="booking" />
      </div>
    </div>
  </div>
</template>
