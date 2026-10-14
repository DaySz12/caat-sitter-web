<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { ChevronLeft, ChevronRight, ExternalLink } from 'lucide-vue-next';
import { API_URL } from '@/shared/lib/api';
import type { BannerDto } from '@/shared/types/models';

const props = defineProps<{
  banners: BannerDto[];
}>();

const router = useRouter();
const currentIndex = ref(0);
let timer: any = null;

function resolveImageUrl(url: string) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
}

function nextSlide() {
  if (props.banners.length <= 1) return;
  currentIndex.value = (currentIndex.value + 1) % props.banners.length;
}

function prevSlide() {
  if (props.banners.length <= 1) return;
  currentIndex.value = (currentIndex.value - 1 + props.banners.length) % props.banners.length;
}

function goToSlide(index: number) {
  currentIndex.value = index;
  resetTimer();
}

function startTimer() {
  if (props.banners.length > 1) {
    timer = setInterval(nextSlide, 5000);
  }
}

function stopTimer() {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
}

function resetTimer() {
  stopTimer();
  startTimer();
}

function handleBannerClick(banner: BannerDto) {
  if (!banner.linkUrl) return;
  if (banner.linkUrl.startsWith('http://') || banner.linkUrl.startsWith('https://')) {
    window.open(banner.linkUrl, '_blank');
  } else {
    router.push(banner.linkUrl);
  }
}

onMounted(() => {
  startTimer();
});

onUnmounted(() => {
  stopTimer();
});
</script>

<template>
  <div
    v-if="banners.length > 0"
    class="relative w-full rounded-2xl overflow-hidden shadow-sm group mb-6 bg-slate-900 select-none"
    @mouseenter="stopTimer"
    @mouseleave="startTimer"
  >
    <!-- Slides Container -->
    <div class="relative w-full aspect-21/9 sm:aspect-16/6 min-h-[160px] max-h-[300px]">
      <div
        v-for="(banner, idx) in banners"
        :key="banner.id"
        :class="[
          'absolute inset-0 transition-opacity duration-700 ease-in-out cursor-pointer',
          idx === currentIndex ? 'opacity-100 z-10' : 'opacity-0 z-0 pointer-events-none'
        ]"
        @click="handleBannerClick(banner)"
      >
        <img
          :src="resolveImageUrl(banner.imageUrl)"
          :alt="banner.title"
          class="w-full h-full object-cover"
          @error="(e) => ((e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=1200')"
        />

        <!-- Gradient Vignette Overlay for Title -->
        <div class="absolute inset-0 bg-gradient-to-t from-black/75 via-black/20 to-transparent flex items-end p-5 sm:p-6">
          <div class="text-white max-w-xl">
            <h3 class="text-base sm:text-xl font-bold tracking-tight drop-shadow-sm line-clamp-1 sm:line-clamp-2">
              {{ banner.title }}
            </h3>
            <p v-if="banner.linkUrl" class="text-xs sm:text-sm text-orange-200 mt-1 flex items-center gap-1 font-medium">
              ดูรายละเอียดเพิ่มเติม
              <ExternalLink class="w-3.5 h-3.5" />
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Navigation Arrows -->
    <template v-if="banners.length > 1">
      <button
        type="button"
        @click.stop="prevSlide(); resetTimer();"
        class="absolute left-3 top-1/2 -translate-y-1/2 z-20 w-9 h-9 rounded-full bg-black/40 hover:bg-black/70 text-white flex items-center justify-center backdrop-blur-xs transition-all opacity-0 group-hover:opacity-100 cursor-pointer"
        aria-label="Previous slide"
      >
        <ChevronLeft class="w-5 h-5" />
      </button>

      <button
        type="button"
        @click.stop="nextSlide(); resetTimer();"
        class="absolute right-3 top-1/2 -translate-y-1/2 z-20 w-9 h-9 rounded-full bg-black/40 hover:bg-black/70 text-white flex items-center justify-center backdrop-blur-xs transition-all opacity-0 group-hover:opacity-100 cursor-pointer"
        aria-label="Next slide"
      >
        <ChevronRight class="w-5 h-5" />
      </button>

      <!-- Pagination Indicators -->
      <div class="absolute bottom-3 right-4 z-20 flex items-center gap-1.5">
        <button
          v-for="(_, idx) in banners"
          :key="idx"
          type="button"
          @click.stop="goToSlide(idx)"
          :class="[
            'h-2 rounded-full transition-all duration-300 cursor-pointer',
            idx === currentIndex ? 'w-6 bg-[#FF7A59]' : 'w-2 bg-white/60 hover:bg-white'
          ]"
          :aria-label="`Slide ${idx + 1}`"
        />
      </div>
    </template>
  </div>
</template>
