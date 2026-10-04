<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter, useRoute, RouterLink, RouterView } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { api } from '../api/client';
import {
  LayoutDashboard,
  UserCheck,
  CalendarCheck,
  Users,
  Settings,
  LogOut,
  Cat,
  Menu,
  X,
  Bell,
  ShieldCheck
} from 'lucide-vue-next';

const authStore = useAuthStore();
const router = useRouter();
const route = useRoute();

const isSidebarOpen = ref(false);
const pendingSittersCount = ref(0);

const navItems = [
  { name: 'แดชบอร์ดภาพรวม', path: '/dashboard', icon: LayoutDashboard },
  { name: 'ตรวจสอบ Sitter', path: '/sitters/verification', icon: UserCheck, badge: true },
  { name: 'จัดการการจอง', path: '/bookings', icon: CalendarCheck },
  { name: 'จัดการผู้ใช้งาน', path: '/users', icon: Users },
  { name: 'ตั้งค่าระบบ', path: '/settings', icon: Settings },
];

async function fetchPendingCount() {
  try {
    const dashboard = await api.get<{ pending_sitters: number }>('/api/admin/dashboard');
    pendingSittersCount.value = dashboard.pending_sitters;
  } catch (e) {
    console.error('Error fetching pending sitters:', e);
  }
}

async function handleLogout() {
  await authStore.logout();
  router.push('/login');
}

onMounted(() => {
  fetchPendingCount();
  // Poll periodically in place of the old Supabase Realtime subscription.
  setInterval(fetchPendingCount, 30000);
});
</script>

<template>
  <div class="min-h-screen bg-[#F8FAFC] flex font-['Prompt',sans-serif]">
    <!-- Mobile Sidebar Overlay -->
    <div
      v-if="isSidebarOpen"
      @click="isSidebarOpen = false"
      class="fixed inset-0 bg-slate-900/40 backdrop-blur-xs z-40 lg:hidden"
    ></div>

    <!-- Sidebar Navigation -->
    <aside
      :class="[
        'fixed lg:sticky top-0 left-0 h-screen w-68 bg-[#1E2238] text-white flex flex-col z-50 transition-transform duration-300 ease-in-out shrink-0',
        isSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
      ]"
    >
      <!-- Logo Header -->
      <div class="h-20 px-6 flex items-center justify-between border-b border-slate-700/50">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-gradient-to-tr from-[#FF7A59] to-[#FF9E85] flex items-center justify-center shadow-lg shadow-[#FF7A59]/30">
            <Cat class="w-6 h-6 text-white" />
          </div>
          <div>
            <h1 class="font-bold text-lg leading-tight tracking-tight text-white">Cat Sitter</h1>
            <p class="text-xs text-[#FF9E85] font-medium flex items-center gap-1">
              <ShieldCheck class="w-3.5 h-3.5" /> Admin Portal
            </p>
          </div>
        </div>
        <button @click="isSidebarOpen = false" class="lg:hidden text-slate-400 hover:text-white p-1">
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Navigation Links -->
      <div class="flex-1 px-4 py-6 space-y-1.5 overflow-y-auto">
        <p class="px-3 text-xs font-semibold uppercase tracking-wider text-slate-400 mb-3">เมนูหลัก</p>
        
        <RouterLink
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          @click="isSidebarOpen = false"
          :class="[
            'flex items-center justify-between px-3.5 py-3 rounded-xl text-sm font-medium transition-all duration-200 group',
            route.path === item.path || (item.path !== '/dashboard' && route.path.startsWith(item.path))
              ? 'bg-[#FF7A59] text-white shadow-md shadow-[#FF7A59]/25 font-semibold'
              : 'text-slate-300 hover:bg-slate-800/80 hover:text-white'
          ]"
        >
          <div class="flex items-center gap-3">
            <component
              :is="item.icon"
              :class="[
                'w-5 h-5 transition-transform duration-200 group-hover:scale-110',
                route.path === item.path ? 'text-white' : 'text-slate-400 group-hover:text-[#FF7A59]'
              ]"
            />
            <span>{{ item.name }}</span>
          </div>

          <span
            v-if="item.badge && pendingSittersCount > 0"
            class="px-2 py-0.5 text-xs font-bold rounded-full bg-[#E63946] text-white animate-pulse"
          >
            {{ pendingSittersCount }}
          </span>
        </RouterLink>
      </div>

      <!-- Admin User Footer -->
      <div class="p-4 border-t border-slate-700/50 bg-slate-900/40">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-3 overflow-hidden">
            <div class="w-9 h-9 rounded-full bg-[#FF7A59]/20 border border-[#FF7A59]/40 flex items-center justify-center shrink-0">
              <span class="text-sm font-bold text-[#FF7A59]">AD</span>
            </div>
            <div class="truncate">
              <p class="text-sm font-semibold text-white truncate">{{ authStore.profile?.name || 'Administrator' }}</p>
              <p class="text-xs text-slate-400 truncate">{{ authStore.profile?.email }}</p>
            </div>
          </div>
          <button
            @click="handleLogout"
            title="ออกจากระบบ"
            class="p-2 text-slate-400 hover:text-[#E63946] hover:bg-red-500/10 rounded-lg transition-colors"
          >
            <LogOut class="w-5 h-5" />
          </button>
        </div>
      </div>
    </aside>

    <!-- Main Content Area -->
    <div class="flex-1 flex flex-col min-w-0">
      <!-- Top Navbar -->
      <header class="h-20 bg-white border-b border-slate-200/80 px-6 lg:px-8 flex items-center justify-between sticky top-0 z-30 shadow-xs">
        <div class="flex items-center gap-3">
          <button
            @click="isSidebarOpen = true"
            class="lg:hidden p-2 text-slate-600 hover:bg-slate-100 rounded-lg"
          >
            <Menu class="w-6 h-6" />
          </button>
          <div>
            <h2 class="text-xl font-bold text-slate-800">
              {{ navItems.find(i => route.path.startsWith(i.path))?.name || 'Cat Sitter Admin' }}
            </h2>
            <p class="text-xs text-slate-500 hidden sm:block">ระบบบริหารจัดการ Cat Sitter Platform</p>
          </div>
        </div>

        <div class="flex items-center gap-4">
          <!-- Realtime Indicator -->
          <div class="hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-full bg-emerald-50 border border-emerald-200 text-xs font-medium text-emerald-700">
            <span class="w-2 h-2 rounded-full bg-emerald-500 animate-ping"></span>
            <span>API Connected</span>
          </div>

          <!-- Notification Bell -->
          <RouterLink
            to="/sitters/verification"
            class="relative p-2.5 text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
          >
            <Bell class="w-5 h-5" />
            <span
              v-if="pendingSittersCount > 0"
              class="absolute top-1.5 right-1.5 w-2.5 h-2.5 bg-[#E63946] rounded-full ring-2 ring-white"
            ></span>
          </RouterLink>
        </div>
      </header>

      <!-- Page Content -->
      <main class="flex-1 p-6 lg:p-8 max-w-7xl w-full mx-auto">
        <RouterView />
      </main>
    </div>
  </div>
</template>
