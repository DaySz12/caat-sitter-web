<script setup lang="ts">
withDefaults(defineProps<{ photos: string[]; sizeClass?: string; alt?: string }>(), { sizeClass: 'w-16 h-16', alt: 'รูปภาพ' });
</script>

<template>
  <div v-if="photos.length" class="flex flex-wrap gap-2">
    <a v-for="(url, i) in photos" :key="url" :href="url" target="_blank" rel="noopener">
      <img :src="url" :alt="`${alt} ${i + 1}`" :class="['rounded-lg object-cover border border-slate-100', sizeClass]" />
    </a>
  </div>
</template>
