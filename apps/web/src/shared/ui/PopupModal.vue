<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import { X, ExternalLink, Sparkles } from 'lucide-vue-next';
import { API_URL } from '@/shared/lib/api';
import type { BannerDto } from '@/shared/types/models';

const props = defineProps<{
  popups: BannerDto[];
}>();

const router = useRouter();
const activePopup = ref<BannerDto | null>(null);
const isVisible = ref(false);
const dontShowToday = ref(false);

function resolveImageUrl(url: string) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
}

function checkAndShowPopup() {
  if (!props.popups || props.popups.length === 0) return;

  const todayStr = new Date().toISOString().slice(0, 10);

  // Find the first popup that has not been dismissed today
  for (const popup of props.popups) {
    const hideDate = localStorage.getItem(`catsitter_hide_popup_${popup.id}`);
    if (hideDate !== todayStr) {
      activePopup.value = popup;
      // Delay slightly for smooth entrance after page loads
      setTimeout(() => {
        isVisible.value = true;
      }, 600);
      break;
    }
  }
}

function closePopup() {
  if (activePopup.value && dontShowToday.value) {
    const todayStr = new Date().toISOString().slice(0, 10);
    localStorage.setItem(`catsitter_hide_popup_${activePopup.value.id}`, todayStr);
  }
  isVisible.value = false;
}

function handlePopupClick() {
  if (!activePopup.value) return;
  const link = activePopup.value.linkUrl;
  closePopup();
  if (link) {
    if (link.startsWith('http://') || link.startsWith('https://')) {
      window.open(link, '_blank');
    } else {
      router.push(link);
    }
  }
}

watch(
  () => props.popups,
  () => {
    checkAndShowPopup();
  },
  { immediate: true, deep: true }
);

onMounted(() => {
  checkAndShowPopup();
});
</script>

<template>
  <div
    v-if="isVisible && activePopup"
    class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-in fade-in duration-300"
    @click.self="closePopup"
  >
    <div
      class="relative bg-white rounded-3xl max-w-md sm:max-w-xl lg:max-w-2xl w-full overflow-hidden shadow-2xl border border-slate-100 flex flex-col animate-in zoom-in-95 duration-300"
    >
      <!-- Close Button -->
      <button
        type="button"
        @click="closePopup"
        class="absolute top-4 right-4 z-20 w-9 h-9 sm:w-10 sm:h-10 rounded-full bg-black/60 hover:bg-black/85 text-white flex items-center justify-center transition-all cursor-pointer shadow-md hover:scale-105"
        aria-label="Close"
      >
        <X class="w-5 h-5" />
      </button>

      <!-- Image -->
      <div
        class="relative w-full aspect-16/10 sm:aspect-16/9 max-h-[360px] bg-slate-900 cursor-pointer overflow-hidden group"
        @click="handlePopupClick"
      >
        <img
          :src="resolveImageUrl(activePopup.imageUrl)"
          :alt="activePopup.title"
          class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
          @error="(e) => ((e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=1200')"
        />

        <div class="absolute top-4 left-4">
          <span class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs sm:text-sm font-semibold bg-purple-600/90 text-white backdrop-blur-xs shadow-md">
            <Sparkles class="w-4 h-4 text-amber-300" />
            ข่าวสาร & โปรโมชั่น
          </span>
        </div>
      </div>

      <!-- Content Details -->
      <div class="p-6 sm:p-7 flex flex-col justify-between">
        <div>
          <h3 class="text-lg sm:text-xl font-bold text-slate-800 leading-snug">
            {{ activePopup.title }}
          </h3>
        </div>

        <!-- Action Button -->
        <div class="mt-5 flex flex-col gap-3.5">
          <button
            v-if="activePopup.linkUrl"
            type="button"
            @click="handlePopupClick"
            class="w-full py-3.5 px-6 rounded-2xl bg-gradient-to-r from-[#FF7A59] to-[#FF9E85] hover:opacity-95 text-white font-bold text-base shadow-lg shadow-[#FF7A59]/30 flex items-center justify-center gap-2 cursor-pointer transition-all transform hover:-translate-y-0.5 active:translate-y-0"
          >
            <span>ดูรายละเอียดสิทธิพิเศษ</span>
            <ExternalLink class="w-4 h-4" />
          </button>

          <!-- Dismiss Option -->
          <div class="flex items-center justify-between text-xs sm:text-sm text-slate-500 pt-1">
            <label class="flex items-center gap-2 cursor-pointer select-none hover:text-slate-700">
              <input
                v-model="dontShowToday"
                type="checkbox"
                class="w-4 h-4 rounded text-[#FF7A59] focus:ring-[#FF7A59] cursor-pointer"
              />
              <span>ไม่ต้องแสดงอีกในวันนี้</span>
            </label>

            <button
              type="button"
              @click="closePopup"
              class="text-slate-400 hover:text-slate-600 underline cursor-pointer"
            >
              ปิดหน้าต่าง
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
