<script setup lang="ts">
import { RouterView, RouterLink, useRouter } from 'vue-router';
import { Home, Briefcase, ClipboardList, User, LogOut, PawPrint } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';

const auth = useAuthStore();
const router = useRouter();

const navItems = [
  { to: '/sitter', label: 'หน้าแรก', icon: Home },
  { to: '/sitter/jobs', label: 'งานของฉัน', icon: Briefcase },
  { to: '/sitter/requests', label: 'คำขอจอง', icon: ClipboardList },
  { to: '/sitter/profile', label: 'โปรไฟล์', icon: User },
];

function handleLogout() {
  auth.logout();
  router.push('/login');
}
</script>

<template>
  <div class="min-h-screen bg-[#FDFBF9]">
    <header class="sticky top-0 z-20 bg-white border-b border-slate-100 shadow-sm">
      <div class="max-w-6xl mx-auto px-4 h-16 flex items-center justify-between">
        <RouterLink to="/sitter" class="flex items-center gap-2">
          <div class="w-9 h-9 rounded-xl bg-gradient-to-tr from-[#1E2238] to-[#2E3456] flex items-center justify-center">
            <PawPrint class="w-5 h-5 text-white" />
          </div>
          <span class="font-bold text-slate-800 text-lg">Cat Sitter</span>
          <span class="text-xs px-2 py-0.5 rounded-full bg-[#1E2238] text-white">Sitter</span>
        </RouterLink>

        <nav class="hidden md:flex items-center gap-1">
          <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium text-slate-500 hover:bg-slate-100 hover:text-[#1E2238] transition"
            active-class="!bg-slate-100 !text-[#1E2238]"
          >
            <component :is="item.icon" class="w-4 h-4" />
            {{ item.label }}
          </RouterLink>
        </nav>

        <div class="flex items-center gap-3">
          <span class="hidden sm:block text-sm text-slate-600">สวัสดี, {{ auth.user?.name }}</span>
          <button
            class="flex items-center gap-1.5 text-sm text-slate-500 hover:text-red-500 px-2 py-1.5 rounded-lg hover:bg-red-50 transition"
            @click="handleLogout"
          >
            <LogOut class="w-4 h-4" /> ออกจากระบบ
          </button>
        </div>
      </div>

      <nav class="md:hidden flex overflow-x-auto px-2 pb-2 gap-1">
        <RouterLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium text-slate-500 whitespace-nowrap hover:bg-slate-100 hover:text-[#1E2238]"
          active-class="!bg-slate-100 !text-[#1E2238]"
        >
          <component :is="item.icon" class="w-3.5 h-3.5" />
          {{ item.label }}
        </RouterLink>
      </nav>
    </header>

    <main class="max-w-6xl mx-auto px-4 py-6">
      <RouterView />
    </main>
  </div>
</template>
