<script setup lang="ts">
import type { Component } from 'vue';

defineProps<{ icon: Component; label: string; value: string | number; iconClass: string }>();
</script>

<template>
  <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-4 flex items-center gap-3">
    <div :class="['w-10 h-10 rounded-xl flex items-center justify-center shrink-0', iconClass]">
      <component :is="icon" class="w-5 h-5" />
    </div>
    <div class="min-w-0">
      <p class="text-xs text-slate-500 truncate">{{ label }}</p>
      <p class="text-lg font-bold text-slate-800 leading-tight">{{ value }}</p>
    </div>
  </div>
</template>
