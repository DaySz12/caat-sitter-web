<script setup lang="ts">
import { computed } from 'vue';
import { Home, Cat, Search, CalendarCheck, MessageCircle, Bell, User } from 'lucide-vue-next';
import { useSummaryStore } from '@/features/notifications/summary.store';
import SidebarLayout from './SidebarLayout.vue';

const summaryStore = useSummaryStore();

const navItems = computed(() => [
  { to: '/owner', label: 'หน้าแรก', icon: Home },
  { to: '/owner/cats', label: 'แมวของฉัน', icon: Cat },
  { to: '/owner/sitters', label: 'ค้นหาผู้รับฝาก', icon: Search },
  { to: '/owner/bookings', label: 'การจองของฉัน', icon: CalendarCheck },
  { to: '/owner/chat', label: 'ข้อความ', icon: MessageCircle, badge: summaryStore.summary?.unreadMessages },
  { to: '/owner/notifications', label: 'การแจ้งเตือน', icon: Bell, badge: summaryStore.summary?.unreadNotifications },
  { to: '/owner/profile', label: 'โปรไฟล์', icon: User },
]);
</script>

<template>
  <SidebarLayout
    :nav-items="navItems"
    home-path="/owner"
    notifications-path="/owner/notifications"
    logo-class="bg-gradient-to-tr from-[#FF7A59] to-[#FF9E85]"
    link-class="hover:bg-[#FFF0EB] hover:text-[#FF7A59]"
    active-class="bg-[#FFF0EB] text-[#FF7A59]"
  />
</template>
