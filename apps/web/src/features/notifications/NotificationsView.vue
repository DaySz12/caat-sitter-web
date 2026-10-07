<script setup lang="ts">
import { ref, computed, onMounted, type Component } from 'vue';
import { useRouter } from 'vue-router';
import {
  Bell, ClipboardList, CalendarCheck, Star, MapPin, Home, CheckCircle2, ShieldCheck, ShieldAlert, CheckCheck,
} from 'lucide-vue-next';
import { notificationsApi } from './notifications.api';
import { useSummaryStore } from './summary.store';
import { useAuthStore } from '@/features/auth/auth.store';
import type { NotificationDto } from '@/shared/types/models';
import { formatRelativeTime } from '@/shared/lib/format';
import { bookingDetailPath } from '@/shared/lib/paths';
import { useLiveEvent } from '@/shared/lib/liveEvents';

const router = useRouter();
const auth = useAuthStore();
const summaryStore = useSummaryStore();
const notifications = ref<NotificationDto[]>([]);
const isLoading = ref(true);

useLiveEvent('notification', (notification) => notifications.value.unshift(notification));

const hasUnread = computed(() => notifications.value.some((n) => !n.isRead));

const TYPE_STYLES: Record<string, { icon: Component; class: string }> = {
  booking_created: { icon: ClipboardList, class: 'bg-[#FFF0EB] text-[#FF7A59]' },
  booking_status_changed: { icon: CalendarCheck, class: 'bg-blue-50 text-blue-600' },
  review_received: { icon: Star, class: 'bg-amber-50 text-amber-500' },
  care_check_in: { icon: MapPin, class: 'bg-green-50 text-green-600' },
  care_check_out: { icon: Home, class: 'bg-slate-100 text-slate-600' },
  care_checklist_done: { icon: CheckCircle2, class: 'bg-green-50 text-green-600' },
  sitter_approved: { icon: ShieldCheck, class: 'bg-green-50 text-green-600' },
  sitter_rejected: { icon: ShieldAlert, class: 'bg-red-50 text-red-500' },
};
const DEFAULT_STYLE = { icon: Bell, class: 'bg-slate-100 text-slate-500' };

function styleOf(type: string) {
  return TYPE_STYLES[type] ?? DEFAULT_STYLE;
}

async function open(notification: NotificationDto) {
  if (!notification.isRead) {
    notification.isRead = true;
    await notificationsApi.markRead(notification.id);
    summaryStore.refresh();
  }
  if (notification.bookingId) router.push(bookingDetailPath(auth.user?.role, notification.bookingId));
}

async function markAllRead() {
  await notificationsApi.markAllRead();
  notifications.value.forEach((n) => (n.isRead = true));
  summaryStore.refresh();
}

onMounted(async () => {
  notifications.value = await notificationsApi.list();
  isLoading.value = false;
});
</script>

<template>
  <div class="max-w-2xl">
    <div class="flex items-start justify-between gap-3 mb-5">
      <div>
        <h1 class="text-xl font-bold text-slate-800 mb-1">การแจ้งเตือน</h1>
        <p class="text-sm text-slate-500">อัปเดตการจอง รีวิว และงานดูแลน้องแมว</p>
      </div>
      <button
        v-if="hasUnread"
        class="flex items-center gap-1.5 text-xs font-semibold text-slate-500 hover:text-slate-700 px-3 py-2 rounded-lg hover:bg-slate-100 shrink-0"
        @click="markAllRead"
      >
        <CheckCheck class="w-4 h-4" /> อ่านทั้งหมด
      </button>
    </div>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>
    <div v-else-if="notifications.length === 0" class="text-center py-16 bg-white rounded-2xl border border-dashed border-slate-200 text-slate-500 text-sm">
      ยังไม่มีการแจ้งเตือน
    </div>
    <div v-else class="bg-white rounded-2xl border border-slate-100 shadow-sm divide-y divide-slate-100 overflow-hidden">
      <button
        v-for="n in notifications"
        :key="n.id"
        class="w-full flex items-start gap-3 p-4 text-left transition hover:bg-slate-50"
        :class="{ 'bg-[#FFF8F5]': !n.isRead }"
        @click="open(n)"
      >
        <div :class="['w-10 h-10 rounded-xl flex items-center justify-center shrink-0', styleOf(n.type).class]">
          <component :is="styleOf(n.type).icon" class="w-5 h-5" />
        </div>
        <div class="flex-1 min-w-0">
          <p class="text-sm text-slate-800" :class="n.isRead ? 'font-medium' : 'font-semibold'">{{ n.title }}</p>
          <p class="text-sm text-slate-500 mt-0.5">{{ n.body }}</p>
          <p class="text-xs text-slate-400 mt-1">{{ formatRelativeTime(n.createdAt) }}</p>
        </div>
        <span v-if="!n.isRead" class="w-2.5 h-2.5 rounded-full bg-[#FF7A59] mt-1.5 shrink-0" />
      </button>
    </div>
  </div>
</template>
