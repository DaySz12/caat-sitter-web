<script setup lang="ts">
import { Cat as CatIcon, Pencil, Trash2 } from 'lucide-vue-next';
import type { CatDto } from '@/shared/types/models';
import { ageDisplay } from '@/shared/lib/format';

defineProps<{ cat: CatDto }>();
const emit = defineEmits<{ edit: [cat: CatDto]; remove: [cat: CatDto] }>();
</script>

<template>
  <div class="bg-white rounded-2xl border border-slate-100 p-4 shadow-sm flex items-center gap-4">
    <div class="w-16 h-16 rounded-xl bg-[#FFF0EB] shrink-0 overflow-hidden flex items-center justify-center">
      <img v-if="cat.photoUrl" :src="cat.photoUrl" :alt="cat.name" class="w-full h-full object-cover" />
      <CatIcon v-else class="w-8 h-8 text-[#FF7A59]" />
    </div>
    <div class="flex-1 min-w-0">
      <h3 class="font-semibold text-slate-800 truncate">{{ cat.name }}</h3>
      <p class="text-xs text-slate-500">{{ cat.breed || 'ไม่ระบุสายพันธุ์' }} · {{ ageDisplay(cat.ageYears, cat.ageMonths) }}</p>
      <p class="text-xs text-slate-400 mt-0.5">{{ cat.gender === 'male' ? 'เพศผู้' : 'เพศเมีย' }}{{ cat.isNeutered ? ' · ทำหมันแล้ว' : '' }}</p>
    </div>
    <div class="flex items-center gap-1 shrink-0">
      <button class="p-2 rounded-lg hover:bg-slate-100 text-slate-500" @click="emit('edit', cat)">
        <Pencil class="w-4 h-4" />
      </button>
      <button class="p-2 rounded-lg hover:bg-red-50 text-red-500" @click="emit('remove', cat)">
        <Trash2 class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>
