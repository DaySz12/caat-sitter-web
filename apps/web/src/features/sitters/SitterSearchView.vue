<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Search, MapPin, SlidersHorizontal } from 'lucide-vue-next';
import { sittersApi, type SitterSearchParams } from './sitters.api';
import SitterCard from './SitterCard.vue';
import type { SitterDto } from '@/shared/types/models';

const sitters = ref<SitterDto[]>([]);
const isLoading = ref(true);
const search = ref('');
const sortBy = ref<SitterSearchParams['sortBy']>('smart');
const maxPrice = ref<number | null>(null);
const userCoords = ref<{ lat: number; lng: number } | null>(null);
const locating = ref(false);

async function load() {
  isLoading.value = true;
  try {
    sitters.value = await sittersApi.search({
      search: search.value || undefined,
      maxPrice: maxPrice.value || undefined,
      sortBy: sortBy.value,
      userLat: userCoords.value?.lat,
      userLng: userCoords.value?.lng,
    });
  } finally {
    isLoading.value = false;
  }
}

function useMyLocation() {
  if (!navigator.geolocation) return;
  locating.value = true;
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      userCoords.value = { lat: pos.coords.latitude, lng: pos.coords.longitude };
      locating.value = false;
      load();
    },
    () => {
      locating.value = false;
    }
  );
}

onMounted(load);
</script>

<template>
  <div>
    <div class="mb-5">
      <h1 class="text-xl font-bold text-slate-800">ค้นหาผู้รับฝากเลี้ยงแมว</h1>
      <p class="text-sm text-slate-500">จัดอันดับด้วย Smart Matching: คะแนนรีวิว, ระยะทาง, ราคา</p>
    </div>

    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-4 mb-5 space-y-3">
      <div class="relative">
        <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
        <input
          v-model="search"
          type="text"
          placeholder="ค้นหาชื่อ, ย่าน, หรือคำอธิบาย..."
          class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]"
          @keyup.enter="load"
        />
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <select v-model="sortBy" class="px-3 py-2 rounded-lg border border-slate-200 text-xs" @change="load">
          <option value="smart">แนะนำ (Smart Match)</option>
          <option value="distance">ใกล้ที่สุด</option>
          <option value="rating">คะแนนสูงสุด</option>
          <option value="price_asc">ราคาต่ำสุด</option>
        </select>

        <input
          v-model.number="maxPrice"
          type="number"
          placeholder="ราคาสูงสุด/วัน"
          class="w-36 px-3 py-2 rounded-lg border border-slate-200 text-xs"
          @change="load"
        />

        <button
          class="flex items-center gap-1 px-3 py-2 rounded-lg border text-xs"
          :class="userCoords ? 'bg-[#FFF0EB] border-[#FF7A59] text-[#E05638]' : 'border-slate-200 text-slate-500'"
          @click="useMyLocation"
        >
          <MapPin class="w-3.5 h-3.5" />
          {{ locating ? 'กำลังค้นหา...' : userCoords ? 'ใช้ตำแหน่งของฉันแล้ว' : 'ใช้ตำแหน่งของฉัน' }}
        </button>

        <button class="flex items-center gap-1 px-3 py-2 rounded-lg bg-[#FF7A59] text-white text-xs font-semibold ml-auto" @click="load">
          <SlidersHorizontal class="w-3.5 h-3.5" /> ค้นหา
        </button>
      </div>
    </div>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>
    <div v-else-if="sitters.length === 0" class="text-center py-16 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
      ไม่พบผู้รับฝากเลี้ยงที่ตรงกับเงื่อนไข
    </div>
    <div v-else class="space-y-3">
      <SitterCard v-for="sitter in sitters" :key="sitter.id" :sitter="sitter" />
    </div>
  </div>
</template>
