<script setup lang="ts">
import { statusColorClass, statusLabel } from '@/shared/lib/format';

defineProps<{ status: string }>();
</script>

<template>
  <span
    class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border"
    :class="statusColorClass(status)"
  >
    {{ statusLabel(status) }}
  </span>
</template>
