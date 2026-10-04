<script setup lang="ts">
import { Star } from 'lucide-vue-next';

const props = withDefaults(defineProps<{ rating: number; size?: number }>(), { size: 14 });
</script>

<template>
  <div class="flex items-center gap-0.5">
    <Star
      v-for="i in 5"
      :key="i"
      :size="size"
      :class="i <= Math.round(props.rating) ? 'fill-[#FFC857] text-[#FFC857]' : 'text-slate-200'"
    />
  </div>
</template>
