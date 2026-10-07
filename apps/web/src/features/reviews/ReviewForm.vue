<script setup lang="ts">
import { ref, onUnmounted } from 'vue';
import { Star, ImagePlus, X } from 'lucide-vue-next';
import { reviewsApi } from './reviews.api';
import { api } from '@/shared/lib/api';
import type { ReviewDto } from '@/shared/types/models';

const props = defineProps<{ bookingId: string; sitterName?: string }>();
const emit = defineEmits<{ submitted: [review: ReviewDto] }>();

const LABELS = ['', 'ควรปรับปรุง', 'พอใช้', 'ดี', 'ดีมาก', 'ประทับใจที่สุด'];
const MAX_PHOTOS = 4;

const rating = ref(0);
const hover = ref(0);
const comment = ref('');
const photos = ref<{ file: File; preview: string }[]>([]);
const isSubmitting = ref(false);
const error = ref<string | null>(null);

function onPhotosPicked(event: Event) {
  const input = event.target as HTMLInputElement;
  const picked = Array.from(input.files ?? []).slice(0, MAX_PHOTOS - photos.value.length);
  photos.value.push(...picked.map((file) => ({ file, preview: URL.createObjectURL(file) })));
  input.value = '';
}

function removePhoto(index: number) {
  URL.revokeObjectURL(photos.value[index].preview);
  photos.value.splice(index, 1);
}

onUnmounted(() => photos.value.forEach((p) => URL.revokeObjectURL(p.preview)));

async function submit() {
  if (rating.value === 0) {
    error.value = 'กรุณาเลือกจำนวนดาว';
    return;
  }
  isSubmitting.value = true;
  error.value = null;
  try {
    const uploaded = await Promise.all(photos.value.map((p) => api.upload('review-photos', p.file)));
    emit(
      'submitted',
      await reviewsApi.create({
        bookingId: props.bookingId,
        rating: rating.value,
        comment: comment.value || undefined,
        photos: uploaded.map((u) => u.url),
      }),
    );
  } catch (e) {
    error.value = (e as Error).message;
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<template>
  <form class="space-y-3" @submit.prevent="submit">
    <p class="text-sm text-slate-600">การดูแลของ {{ sitterName ?? 'พี่เลี้ยง' }} เป็นอย่างไรบ้าง?</p>
    <div class="flex items-center gap-1" @mouseleave="hover = 0">
      <button
        v-for="i in 5"
        :key="i"
        type="button"
        class="p-0.5"
        :aria-label="`${i} ดาว`"
        @mouseenter="hover = i"
        @click="rating = i"
      >
        <Star :size="30" :class="i <= (hover || rating) ? 'fill-[#FFC857] text-[#FFC857]' : 'text-slate-200'" />
      </button>
      <span class="ml-2 text-sm font-medium text-slate-600">{{ LABELS[hover || rating] }}</span>
    </div>
    <textarea
      v-model="comment"
      rows="3"
      maxlength="500"
      placeholder="เล่าประสบการณ์ให้เจ้าของแมวคนอื่นฟัง (ไม่บังคับ)"
      class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
    />

    <div class="flex flex-wrap gap-2">
      <div v-for="(photo, i) in photos" :key="photo.preview" class="relative">
        <img :src="photo.preview" :alt="`รูปรีวิว ${i + 1}`" class="w-16 h-16 rounded-lg object-cover border border-slate-100" />
        <button
          type="button"
          class="absolute -top-1.5 -right-1.5 w-5 h-5 rounded-full bg-slate-700 text-white flex items-center justify-center"
          aria-label="ลบรูป"
          @click="removePhoto(i)"
        >
          <X class="w-3 h-3" />
        </button>
      </div>
      <label
        v-if="photos.length < MAX_PHOTOS"
        class="w-16 h-16 rounded-lg border-2 border-dashed border-slate-200 text-slate-400 hover:border-[#FF7A59] hover:text-[#FF7A59] flex flex-col items-center justify-center cursor-pointer text-[10px]"
      >
        <ImagePlus class="w-5 h-5" />
        เพิ่มรูป
        <input type="file" accept="image/*" multiple class="hidden" @change="onPhotosPicked" />
      </label>
    </div>

    <p v-if="error" class="text-xs text-red-500">{{ error }}</p>
    <button
      type="submit"
      :disabled="isSubmitting"
      class="px-5 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm disabled:opacity-50"
    >
      {{ isSubmitting ? 'กำลังส่ง...' : 'ส่งรีวิว' }}
    </button>
  </form>
</template>
