<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { Bell, X } from 'lucide-vue-next';
import { notificationsApi } from './notifications.api';
import { useSummaryStore } from './summary.store';
import { useAuthStore } from '@/features/auth/auth.store';
import { useLiveEvent } from '@/shared/lib/liveEvents';
import { bookingDetailPath } from '@/shared/lib/paths';
import type { NotificationDto } from '@/shared/types/models';

const VISIBLE_MS = 6000;
const MAX_TOASTS = 3;

const router = useRouter();
const auth = useAuthStore();
const summaryStore = useSummaryStore();
const toasts = ref<NotificationDto[]>([]);

useLiveEvent('notification', (notification) => {
  toasts.value = [notification, ...toasts.value].slice(0, MAX_TOASTS);
  setTimeout(() => dismiss(notification.id), VISIBLE_MS);
});

function dismiss(id: string) {
  toasts.value = toasts.value.filter((t) => t.id !== id);
}

async function open(notification: NotificationDto) {
  dismiss(notification.id);
  await notificationsApi.markRead(notification.id);
  summaryStore.refresh();
  if (notification.bookingId) router.push(bookingDetailPath(auth.user?.role, notification.bookingId));
}
</script>

<template>
  <div class="fixed z-[60] top-16 lg:top-4 left-4 right-4 sm:left-auto sm:w-96 space-y-2 pointer-events-none" aria-live="polite">
    <TransitionGroup
      enter-from-class="opacity-0 translate-y-[-8px]"
      enter-active-class="transition duration-200"
      leave-to-class="opacity-0"
      leave-active-class="transition duration-200"
    >
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="pointer-events-auto flex items-start gap-3 bg-white rounded-2xl border border-slate-100 shadow-lg p-4 cursor-pointer hover:bg-slate-50"
        @click="open(toast)"
      >
        <div class="w-9 h-9 rounded-xl bg-[#FFF0EB] text-[#FF7A59] flex items-center justify-center shrink-0">
          <Bell class="w-4 h-4" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm font-semibold text-slate-800">{{ toast.title }}</p>
          <p class="text-xs text-slate-500 mt-0.5 line-clamp-2">{{ toast.body }}</p>
        </div>
        <button class="p-1 -m-1 rounded-lg text-slate-400 hover:text-slate-600" aria-label="ปิด" @click.stop="dismiss(toast.id)">
          <X class="w-4 h-4" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>
