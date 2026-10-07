<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(defineProps<{ name?: string | null; src?: string | null; sizeClass?: string }>(), {
  sizeClass: 'w-9 h-9 text-sm',
});

// Skip a leading Thai vowel (เ แ โ ใ ไ) so "เมย์" shows "ม" rather than a lone vowel.
const initial = computed(() => (props.name ?? '').trim().replace(/^[เแโใไ]/, '').charAt(0).toUpperCase());
</script>

<template>
  <div :class="['rounded-full overflow-hidden bg-slate-100 flex items-center justify-center shrink-0 font-bold text-slate-500', sizeClass]">
    <img v-if="src" :src="src" class="w-full h-full object-cover" />
    <span v-else>{{ initial }}</span>
  </div>
</template>
