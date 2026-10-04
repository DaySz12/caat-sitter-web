<script setup lang="ts">
import { User, MapPin, ShieldCheck } from 'lucide-vue-next';
import { useRouter } from 'vue-router';
import type { SitterDto } from '@/shared/types/models';
import { formatPerDay } from '@/shared/lib/format';
import RatingStars from '@/shared/ui/RatingStars.vue';

const props = defineProps<{ sitter: SitterDto }>();
const router = useRouter();
</script>

<template>
  <div
    class="bg-white rounded-2xl border border-slate-100 shadow-sm p-4 flex gap-4 cursor-pointer hover:shadow-md hover:border-[#FF7A59]/30 transition"
    @click="router.push(`/owner/sitters/${sitter.id}`)"
  >
    <div class="w-20 h-20 rounded-xl bg-[#FFF0EB] shrink-0 overflow-hidden flex items-center justify-center">
      <img v-if="sitter.user?.avatarUrl" :src="sitter.user.avatarUrl" class="w-full h-full object-cover" />
      <User v-else class="w-9 h-9 text-[#FF7A59]" />
    </div>

    <div class="flex-1 min-w-0">
      <div class="flex items-center gap-1.5">
        <h3 class="font-semibold text-slate-800 truncate">{{ sitter.user?.name || 'ผู้รับฝากเลี้ยง' }}</h3>
        <ShieldCheck v-if="sitter.isVerified" class="w-4 h-4 text-blue-500 shrink-0" />
      </div>
      <div class="flex items-center gap-2 mt-0.5">
        <RatingStars :rating="sitter.ratingAvg" />
        <span class="text-xs text-slate-500">({{ sitter.reviewCount }})</span>
      </div>
      <p v-if="sitter.address" class="flex items-center gap-1 text-xs text-slate-400 mt-1 truncate">
        <MapPin class="w-3 h-3 shrink-0" /> {{ sitter.address }}
        <span v-if="sitter.distanceKm != null">· {{ sitter.distanceKm.toFixed(1) }} กม.</span>
      </p>
      <div class="flex flex-wrap gap-1 mt-2">
        <span
          v-for="skill in sitter.specialSkills.slice(0, 2)"
          :key="skill"
          class="text-[10px] px-2 py-0.5 rounded-full bg-[#FFF0EB] text-[#E05638]"
        >
          {{ skill }}
        </span>
      </div>
    </div>

    <div class="text-right shrink-0">
      <p class="font-bold text-[#FF7A59]">{{ formatPerDay(sitter.pricePerDay) }}</p>
      <p v-if="sitter.matchScore != null" class="text-[10px] text-slate-400 mt-1">Match {{ sitter.matchScore }}%</p>
    </div>
  </div>
</template>
