<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { User, MapPin, ShieldCheck, ArrowLeft, Briefcase, Cat as CatIcon, MessageCircle, Star } from 'lucide-vue-next';
import { sittersApi } from './sitters.api';
import { reviewsApi } from '@/features/reviews/reviews.api';
import { chatApi } from '@/features/chat/chat.api';
import ReviewList from '@/features/reviews/ReviewList.vue';
import type { ReviewDto, SitterDto } from '@/shared/types/models';
import { formatPerDay } from '@/shared/lib/format';
import RatingStars from '@/shared/ui/RatingStars.vue';

const route = useRoute();
const router = useRouter();
const sitter = ref<SitterDto | null>(null);
const reviews = ref<ReviewDto[]>([]);
const isLoading = ref(true);

async function openChat() {
  if (!sitter.value) return;
  const room = await chatApi.start(sitter.value.id);
  router.push(`/owner/chat/${room.id}`);
}

onMounted(async () => {
  const id = route.params.id as string;
  [sitter.value, reviews.value] = await Promise.all([sittersApi.getById(id), reviewsApi.forSitter(id)]);
  isLoading.value = false;
});
</script>

<template>
  <div class="max-w-2xl mx-auto">
    <button class="flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-700 mb-4" @click="router.back()">
      <ArrowLeft class="w-4 h-4" /> กลับ
    </button>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>

    <div v-else-if="sitter" class="space-y-5">
    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden">
      <div class="h-24 bg-gradient-to-r from-[#FF7A59] to-[#FFC857]"></div>
      <div class="px-6 pb-6">
        <div class="-mt-10 mb-3">
          <div class="w-20 h-20 rounded-2xl bg-white p-1 shadow-md">
            <div class="w-full h-full rounded-xl bg-[#FFF0EB] overflow-hidden flex items-center justify-center">
              <img v-if="sitter.user?.avatarUrl" :src="sitter.user.avatarUrl" class="w-full h-full object-cover" />
              <User v-else class="w-9 h-9 text-[#FF7A59]" />
            </div>
          </div>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <h1 class="text-lg font-bold text-slate-800">{{ sitter.user?.name }}</h1>
          <span v-if="sitter.isVerified" class="inline-flex items-center gap-1 text-xs text-emerald-700 bg-emerald-50 border border-emerald-200/60 px-2 py-0.5 rounded-full font-semibold">
            <ShieldCheck class="w-3.5 h-3.5 text-emerald-600" /> ยืนยันตัวตนแล้ว
          </span>
        </div>
        <div class="flex items-center gap-2 mt-1">
          <RatingStars :rating="sitter.ratingAvg" />
          <span class="text-xs text-slate-500">{{ sitter.ratingAvg.toFixed(1) }} ({{ sitter.reviewCount }} รีวิว)</span>
        </div>
        <p v-if="sitter.address" class="flex items-center gap-1 text-sm text-slate-500 mt-2">
          <MapPin class="w-4 h-4" /> {{ sitter.address }}
        </p>
        <p v-if="sitter.experienceYears" class="flex items-center gap-1 text-sm text-slate-500 mt-1">
          <Briefcase class="w-4 h-4" /> ประสบการณ์ {{ sitter.experienceYears }} ปี · รับดูแลสูงสุด {{ sitter.maxCatsPerDay }} ตัว/วัน
        </p>

        <p v-if="sitter.bio" class="text-sm text-slate-600 mt-4 leading-relaxed">{{ sitter.bio }}</p>

        <div class="flex flex-wrap gap-1.5 mt-4">
          <span v-for="skill in sitter.specialSkills" :key="skill" class="text-xs px-2.5 py-1 rounded-full bg-[#FFF0EB] text-[#E05638]">
            {{ skill }}
          </span>
        </div>

        <div v-if="sitter.servicePhotos.length" class="grid grid-cols-3 gap-2 mt-4">
          <img
            v-for="(photo, i) in sitter.servicePhotos"
            :key="i"
            :src="photo"
            class="w-full aspect-square object-cover rounded-lg"
          />
        </div>

        <div class="flex items-center justify-between mt-6 pt-5 border-t border-slate-100">
          <div>
            <p class="text-xs text-slate-400">อัตราค่าบริการ</p>
            <p class="text-xl font-bold text-[#FF7A59]">{{ formatPerDay(sitter.pricePerDay) }}</p>
          </div>
          <div class="flex gap-2">
            <button
              class="flex items-center gap-2 px-4 py-3 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-semibold text-sm"
              @click="openChat"
            >
              <MessageCircle class="w-4 h-4" /> <span class="hidden sm:inline">ส่งข้อความ</span>
            </button>
            <button
              class="flex items-center gap-2 px-5 py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm"
              @click="router.push(`/owner/booking/new?sitterId=${sitter.id}`)"
            >
              <CatIcon class="w-4 h-4" /> จองบริการ
            </button>
          </div>
        </div>
      </div>
    </div>

    <section class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
      <div class="flex items-center justify-between mb-4">
        <h2 class="font-semibold text-slate-800 flex items-center gap-2"><Star class="w-5 h-5 text-[#FFC857]" /> รีวิวจากเจ้าของแมว</h2>
        <span v-if="reviews.length" class="text-sm text-slate-500">{{ sitter.ratingAvg.toFixed(1) }} · {{ sitter.reviewCount }} รีวิว</span>
      </div>
      <ReviewList v-if="reviews.length" :reviews="reviews" />
      <p v-else class="text-sm text-slate-500 text-center py-6">ยังไม่มีรีวิว</p>
    </section>
    </div>
  </div>
</template>
