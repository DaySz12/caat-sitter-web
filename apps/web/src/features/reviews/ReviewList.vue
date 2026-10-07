<script setup lang="ts">
import type { ReviewDto } from '@/shared/types/models';
import { formatRelativeTime } from '@/shared/lib/format';
import RatingStars from '@/shared/ui/RatingStars.vue';
import UserAvatar from '@/shared/ui/UserAvatar.vue';
import PhotoStrip from '@/shared/ui/PhotoStrip.vue';

defineProps<{ reviews: ReviewDto[] }>();
</script>

<template>
  <div class="divide-y divide-slate-100">
    <div v-for="review in reviews" :key="review.id" class="py-4 first:pt-0 last:pb-0 flex gap-3">
      <UserAvatar :name="review.reviewer?.name" :src="review.reviewer?.avatarUrl" />
      <div class="flex-1 min-w-0">
        <div class="flex items-center justify-between gap-2">
          <p class="text-sm font-semibold text-slate-800 truncate">{{ review.reviewer?.name ?? 'เจ้าของแมว' }}</p>
          <span class="text-xs text-slate-400 shrink-0">{{ formatRelativeTime(review.createdAt) }}</span>
        </div>
        <RatingStars :rating="review.rating" :size="13" class="mt-0.5" />
        <p v-if="review.comment" class="text-sm text-slate-600 mt-1.5 leading-relaxed">{{ review.comment }}</p>
        <PhotoStrip :photos="review.photos" alt="รูปรีวิว" size-class="w-14 h-14" class="mt-2" />
      </div>
    </div>
  </div>
</template>
