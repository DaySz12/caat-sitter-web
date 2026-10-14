<script setup lang="ts">
import { ref, onMounted } from 'vue';
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
      class="relative bg-white rounded-3xl max-w-sm sm:max-w-md w-full overflow-hidden shadow-2xl border border-slate-100 flex flex-col animate-in zoom-in-95 duration-300"
    >
      <!-- Close Button -->
      <button
        type="button"
        @click="closePopup"
        class="absolute top-3 right-3 z-20 w-8 h-8 rounded-full bg-black/50 hover:bg-black/80 text-white flex items-center justify-center transition-colors cursor-pointer"
        aria-label="Close"
      >
        <X class="w-4 h-4" />
      </button>

      <!-- Image -->
      <div
        class="relative w-full aspect-4/3 sm:aspect-16/10 bg-slate-900 cursor-pointer overflow-hidden group"
        @click="handlePopupClick"
      >
        <img
          :src="resolveImageUrl(activePopup.imageUrl)"
          :alt="activePopup.title"
          class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
          @error="(e) => ((e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=800')"
        />

        <div class="absolute top-3 left-3">
          <span class="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-purple-600/90 text-white backdrop-blur-xs shadow-xs">
            <Sparkles class="w-3.5 h-3.5 text-amber-300" />
            ข่าวสาร & โปรโมชั่น
          </span>
        </div>
      </div>

      <!-- Content Details -->
      <div class="p-5 sm:p-6 flex flex-col justify-between">
        <div>
          <h3 class="text-base sm:text-lg font-bold text-slate-800 leading-snug">
            {{ activePopup.title }}
          </h3>
        </div>

        <!-- Action Button -->
        <div class="mt-4 flex flex-col gap-3">
          <button
            v-if="activePopup.linkUrl"
            type="button"
            @click="handlePopupClick"
            class="w-full py-2.5 px-4 rounded-xl bg-gradient-to-r from-[#FF7A59] to-[#FF9E85] hover:opacity-95 text-white font-semibold text-sm shadow-md shadow-[#FF7A59]/25 flex items-center justify-center gap-2 cursor-pointer transition-all"
          >
            <span>ดูรายละเอียดสิทธิพิเศษ</span>
            <ExternalLink class="w-4 h-4" />
          </button>

          <!-- Dismiss Option -->
          <div class="flex items-center justify-between text-xs text-slate-500 pt-1">
            <label class="flex items-center gap-2 cursor-pointer select-none">
              <input
                v-model="dontShowToday"
                type="checkbox"
                class="w-4 h-4 rounded text-[#FF7A59] focus:ring-[#FF7A59]"
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
