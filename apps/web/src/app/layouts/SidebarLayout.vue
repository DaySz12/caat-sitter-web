<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, type Component } from 'vue';
import { RouterView, RouterLink, useRoute, useRouter } from 'vue-router';
import { Bell, LogOut, Menu, PawPrint, X } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { useSummaryStore } from '@/features/notifications/summary.store';
import NotificationToast from '@/features/notifications/NotificationToast.vue';
import { connectLiveEvents, disconnectLiveEvents, useLiveEvent } from '@/shared/lib/liveEvents';
import UserAvatar from '@/shared/ui/UserAvatar.vue';

const SUMMARY_POLL_MS = 30_000;

const props = defineProps<{
  navItems: { to: string; label: string; icon: Component; badge?: number }[];
  homePath: string;
  notificationsPath: string;
  /** Background classes for the logo tile. */
  logoClass: string;
  /** Hover classes for inactive nav links. */
  linkClass: string;
  /** Classes for the active nav link. */
  activeClass: string;
  badge?: string;
  /** Optional highlighted figure above the user card, e.g. this month's earnings. */
  footerStat?: { label: string; value: string };
}>();

const auth = useAuthStore();
const summaryStore = useSummaryStore();
const route = useRoute();
const router = useRouter();

const isSidebarOpen = ref(false);
let pollTimer: ReturnType<typeof setInterval> | undefined;

// Close the mobile drawer and refresh badge counts after navigating.
watch(
  () => route.fullPath,
  () => {
    isSidebarOpen.value = false;
    summaryStore.refresh();
  },
);

// Pushed events update the badges right away; the poll only catches up after a dropped stream.
useLiveEvent('notification', summaryStore.refresh);
useLiveEvent('message', () => {
  // The chat page refreshes the badges itself once it has marked the message read.
  if (!route.path.includes('/chat')) summaryStore.refresh();
});

onMounted(() => {
  connectLiveEvents();
  summaryStore.refresh();
  pollTimer = setInterval(summaryStore.refresh, SUMMARY_POLL_MS);
});

onUnmounted(() => {
  clearInterval(pollTimer);
  disconnectLiveEvents();
});

// Keep a section highlighted on its sub-pages too (e.g. /owner/cats/new under /owner/cats).
function isActive(to: string) {
  return route.path === to || (to !== props.homePath && route.path.startsWith(`${to}/`));
}

function handleLogout() {
  disconnectLiveEvents();
  auth.logout();
  summaryStore.clear();
  router.push('/login');
}
</script>

<template>
  <div class="min-h-screen bg-[#FDFBF9] lg:flex">
    <!-- Mobile top bar -->
    <header class="lg:hidden sticky top-0 z-30 h-14 px-4 bg-white border-b border-slate-100 shadow-sm flex items-center gap-3">
      <button
        class="p-2 -ml-2 rounded-lg text-slate-600 hover:bg-slate-100"
        aria-label="เปิดเมนู"
        @click="isSidebarOpen = true"
      >
        <Menu class="w-5 h-5" />
      </button>
      <RouterLink :to="homePath" class="flex items-center gap-2">
        <div :class="['w-8 h-8 rounded-xl flex items-center justify-center', logoClass]">
          <PawPrint class="w-4 h-4 text-white" />
        </div>
        <span class="font-bold text-slate-800">Cat Sitter</span>
      </RouterLink>
      <RouterLink :to="notificationsPath" class="relative ml-auto p-2 -mr-2 rounded-lg text-slate-600 hover:bg-slate-100" aria-label="การแจ้งเตือน">
        <Bell class="w-5 h-5" />
        <span
          v-if="summaryStore.summary?.unreadNotifications"
          class="absolute top-1 right-1 min-w-4 h-4 px-1 rounded-full bg-[#E63946] text-white text-[10px] font-bold flex items-center justify-center"
        >
          {{ summaryStore.summary.unreadNotifications }}
        </span>
      </RouterLink>
    </header>

    <!-- Mobile overlay -->
    <div v-if="isSidebarOpen" class="fixed inset-0 z-40 bg-slate-900/40 lg:hidden" @click="isSidebarOpen = false" />

    <aside
      :class="[
        'fixed lg:sticky top-0 left-0 z-50 h-screen w-64 shrink-0 bg-white border-r border-slate-100 flex flex-col transition-transform duration-300 lg:translate-x-0',
        isSidebarOpen ? 'translate-x-0' : '-translate-x-full',
      ]"
    >
      <div class="h-16 px-5 flex items-center justify-between border-b border-slate-100">
        <RouterLink :to="homePath" class="flex items-center gap-2">
          <div :class="['w-9 h-9 rounded-xl flex items-center justify-center', logoClass]">
            <PawPrint class="w-5 h-5 text-white" />
          </div>
          <span class="font-bold text-slate-800 text-lg">Cat Sitter</span>
          <span v-if="badge" class="text-xs px-2 py-0.5 rounded-full bg-[#1E2238] text-white">{{ badge }}</span>
        </RouterLink>
        <button
          class="lg:hidden p-1 rounded-lg text-slate-400 hover:text-slate-600"
          aria-label="ปิดเมนู"
          @click="isSidebarOpen = false"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <nav class="flex-1 overflow-y-auto px-3 py-4 space-y-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          :class="[
            'flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition',
            isActive(item.to) ? activeClass : ['text-slate-500', linkClass],
          ]"
          @click="isSidebarOpen = false"
        >
          <component :is="item.icon" class="w-5 h-5" />
          <span class="flex-1">{{ item.label }}</span>
          <span
            v-if="item.badge"
            class="min-w-5 h-5 px-1.5 rounded-full bg-[#E63946] text-white text-[11px] font-bold flex items-center justify-center"
          >
            {{ item.badge > 99 ? '99+' : item.badge }}
          </span>
        </RouterLink>
      </nav>

      <div class="p-4 border-t border-slate-100">
        <div v-if="footerStat" class="rounded-xl bg-slate-50 px-3 py-2.5 mb-3">
          <p class="text-[11px] text-slate-500">{{ footerStat.label }}</p>
          <p class="text-base font-bold text-slate-800">{{ footerStat.value }}</p>
        </div>
        <div class="flex items-center gap-3">
          <UserAvatar :name="auth.user?.name" :src="auth.user?.avatarUrl" />
          <div class="min-w-0">
            <p class="text-sm font-semibold text-slate-800 truncate">{{ auth.user?.name }}</p>
            <p class="text-xs text-slate-400 truncate">{{ auth.user?.email }}</p>
          </div>
        </div>
        <button
          class="mt-3 w-full flex items-center gap-2 px-3 py-2 rounded-xl text-sm text-slate-500 hover:text-red-500 hover:bg-red-50 transition"
          @click="handleLogout"
        >
          <LogOut class="w-4 h-4" /> ออกจากระบบ
        </button>
      </div>
    </aside>

    <main class="flex-1 min-w-0 w-full max-w-6xl mx-auto px-4 lg:px-8 py-6">
      <RouterView />
    </main>

    <NotificationToast />
  </div>
</template>
