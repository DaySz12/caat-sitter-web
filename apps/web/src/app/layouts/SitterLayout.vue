<script setup lang="ts">
import { computed } from 'vue';
import { Home, Briefcase, ClipboardList, MessageCircle, Bell, User } from 'lucide-vue-next';
import { useSummaryStore } from '@/features/notifications/summary.store';
import { formatCurrency } from '@/shared/lib/format';
import SidebarLayout from './SidebarLayout.vue';

const summaryStore = useSummaryStore();

const navItems = computed(() => [
  { to: '/sitter', label: 'หน้าแรก', icon: Home },
  { to: '/sitter/requests', label: 'คำขอจอง', icon: ClipboardList, badge: summaryStore.summary?.pendingRequests },
  { to: '/sitter/jobs', label: 'งานของฉัน', icon: Briefcase },
  { to: '/sitter/chat', label: 'ข้อความ', icon: MessageCircle, badge: summaryStore.summary?.unreadMessages },
  { to: '/sitter/notifications', label: 'การแจ้งเตือน', icon: Bell, badge: summaryStore.summary?.unreadNotifications },
  { to: '/sitter/profile', label: 'โปรไฟล์', icon: User },
]);

const footerStat = computed(() =>
  summaryStore.summary ? { label: 'รายได้เดือนนี้', value: formatCurrency(summaryStore.summary.monthEarnings) } : undefined,
);
</script>

<template>
  <SidebarLayout
    :nav-items="navItems"
    home-path="/sitter"
    notifications-path="/sitter/notifications"
    badge="Sitter"
    :footer-stat="footerStat"
    logo-class="bg-gradient-to-tr from-[#1E2238] to-[#2E3456]"
    link-class="hover:bg-slate-100 hover:text-[#1E2238]"
    active-class="bg-slate-100 text-[#1E2238]"
  />
</template>
