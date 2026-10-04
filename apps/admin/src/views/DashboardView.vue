<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api } from '../api/client';
import {
  CalendarCheck,
  TrendingUp,
  UserCheck,
  Users,
  Clock,
  CheckCircle2,
  ArrowUpRight
} from 'lucide-vue-next';

interface Stats {
  totalBookings: number;
  totalRevenue: number;
  pendingSitters: number;
  totalUsers: number;
  activeBookings: number;
}

const stats = ref<Stats>({
  totalBookings: 0,
  totalRevenue: 0,
  pendingSitters: 0,
  totalUsers: 0,
  activeBookings: 0,
});

const recentBookings = ref<any[]>([]);
const isLoading = ref(true);

async function loadDashboardData() {
  isLoading.value = true;
  try {
    const data = await api.get<{
      total_users: number;
      total_revenue: number;
      pending_sitters: number;
      total_bookings: number;
      active_bookings: number;
      recent_bookings: any[];
    }>('/api/admin/dashboard');

    stats.value = {
      totalBookings: data.total_bookings,
      totalRevenue: data.total_revenue,
      pendingSitters: data.pending_sitters,
      totalUsers: data.total_users,
      activeBookings: data.active_bookings,
    };

    recentBookings.value = data.recent_bookings;
  } catch (e) {
    console.error('Error loading dashboard stats:', e);
  } finally {
    isLoading.value = false;
  }
}

function getStatusBadge(status: string) {
  switch (status) {
    case 'confirmed':
      return { label: 'ยืนยันแล้ว', class: 'bg-emerald-50 text-emerald-700 border-emerald-200' };
    case 'in_progress':
      return { label: 'กำลังดูแล', class: 'bg-blue-50 text-blue-700 border-blue-200' };
    case 'completed':
      return { label: 'เสร็จสิ้น', class: 'bg-purple-50 text-purple-700 border-purple-200' };
    case 'cancelled':
      return { label: 'ยกเลิก', class: 'bg-rose-50 text-rose-700 border-rose-200' };
    default:
      return { label: 'รอยืนยัน', class: 'bg-amber-50 text-amber-700 border-amber-200' };
  }
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('th-TH', { style: 'currency', currency: 'THB', maximumFractionDigits: 0 }).format(amount);
}

onMounted(() => {
  loadDashboardData();
});
</script>

<template>
  <div class="space-y-8">
    <!-- Welcome Header Banner -->
    <div class="bg-gradient-to-r from-[#1E2238] to-[#2E3456] rounded-3xl p-6 sm:p-8 text-white relative overflow-hidden shadow-xl">
      <div class="absolute -right-10 -bottom-10 w-64 h-64 bg-[#FF7A59]/20 rounded-full blur-2xl pointer-events-none"></div>
      <div class="relative z-10 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <span class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-[#FF7A59]/20 text-[#FF9E85] text-xs font-semibold uppercase tracking-wider mb-2 border border-[#FF7A59]/30">
            📊 Overview Statistics
          </span>
          <h1 class="text-2xl sm:text-3xl font-bold">ยินดีต้อนรับสู่แผงควบคุมระบบ</h1>
          <p class="text-slate-300 text-sm mt-1">สรุปข้อมูลกิจกรรมและการเงินแบบ Realtime บนแพลตฟอร์ม Cat Sitter</p>
        </div>
        <RouterLink
          to="/sitters/verification"
          class="inline-flex items-center gap-2 px-5 py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm transition-all shadow-md shadow-[#FF7A59]/30 self-start sm:self-auto"
        >
          <span>ตรวจสอบ Sitter ใหม่</span>
          <span v-if="stats.pendingSitters > 0" class="px-2 py-0.5 text-xs bg-white text-[#FF7A59] rounded-full font-bold">
            {{ stats.pendingSitters }}
          </span>
        </RouterLink>
      </div>
    </div>

    <!-- Stat Cards Grid -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
      <!-- Total Revenue Card -->
      <div class="bg-white rounded-2xl p-6 border border-slate-200/80 shadow-xs flex items-center gap-4 hover:shadow-md transition-shadow">
        <div class="w-14 h-14 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center shrink-0">
          <TrendingUp class="w-7 h-7" />
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">ยอดชำระเงินรวม</p>
          <h3 class="text-2xl font-bold text-slate-800 mt-1">{{ formatCurrency(stats.totalRevenue) }}</h3>
          <p class="text-xs text-emerald-600 font-medium flex items-center gap-1 mt-1">
            <CheckCircle2 class="w-3.5 h-3.5" /> รายได้สะสมบนแพลตฟอร์ม
          </p>
        </div>
      </div>

      <!-- Total Bookings Card -->
      <div class="bg-white rounded-2xl p-6 border border-slate-200/80 shadow-xs flex items-center gap-4 hover:shadow-md transition-shadow">
        <div class="w-14 h-14 rounded-2xl bg-[#FF7A59]/10 text-[#FF7A59] flex items-center justify-center shrink-0">
          <CalendarCheck class="w-7 h-7" />
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">การจองทั้งหมด</p>
          <h3 class="text-2xl font-bold text-slate-800 mt-1">{{ stats.totalBookings.toLocaleString() }} รายการ</h3>
          <p class="text-xs text-[#FF7A59] font-medium flex items-center gap-1 mt-1">
            <Clock class="w-3.5 h-3.5" /> กำลังดูแลอยู่ {{ stats.activeBookings }} งาน
          </p>
        </div>
      </div>

      <!-- Pending Sitters Card -->
      <div class="bg-white rounded-2xl p-6 border border-slate-200/80 shadow-xs flex items-center gap-4 hover:shadow-md transition-shadow">
        <div class="w-14 h-14 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center shrink-0">
          <UserCheck class="w-7 h-7" />
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">Sitter รออนุมัติ</p>
          <h3 class="text-2xl font-bold text-slate-800 mt-1">{{ stats.pendingSitters }} คน</h3>
          <RouterLink to="/sitters/verification" class="text-xs text-amber-600 font-medium hover:underline flex items-center gap-1 mt-1">
            คลิกเพื่อตรวจสอบเอกสาร <ArrowUpRight class="w-3.5 h-3.5" />
          </RouterLink>
        </div>
      </div>

      <!-- Total Users Card -->
      <div class="bg-white rounded-2xl p-6 border border-slate-200/80 shadow-xs flex items-center gap-4 hover:shadow-md transition-shadow">
        <div class="w-14 h-14 rounded-2xl bg-indigo-50 text-indigo-600 flex items-center justify-center shrink-0">
          <Users class="w-7 h-7" />
        </div>
        <div>
          <p class="text-xs font-semibold uppercase tracking-wider text-slate-500">ผู้ใช้งานทั้งหมด</p>
          <h3 class="text-2xl font-bold text-slate-800 mt-1">{{ stats.totalUsers.toLocaleString() }} บัญชี</h3>
          <p class="text-xs text-indigo-600 font-medium flex items-center gap-1 mt-1">
            <Users class="w-3.5 h-3.5" /> Owner & Sitter รวมกัน
          </p>
        </div>
      </div>
    </div>

    <!-- Recent Bookings Table Section -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-xs overflow-hidden">
      <div class="p-6 border-b border-slate-100 flex items-center justify-between">
        <div>
          <h3 class="text-lg font-bold text-slate-800">รายการจองล่าสุด (Recent Bookings)</h3>
          <p class="text-xs text-slate-500 mt-0.5">กิจกรรมการจองและสถานะการดูแลแมวแบบ Realtime</p>
        </div>
        <RouterLink
          to="/bookings"
          class="text-sm font-semibold text-[#FF7A59] hover:text-[#E05638] flex items-center gap-1 transition-colors"
        >
          ดูทั้งหมด <ArrowUpRight class="w-4 h-4" />
        </RouterLink>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-slate-600">
          <thead class="bg-slate-50/75 text-xs uppercase font-semibold text-slate-500 border-b border-slate-100">
            <tr>
              <th class="py-3.5 px-6">รหัสการจอง</th>
              <th class="py-3.5 px-6">เจ้าของแมว (Owner)</th>
              <th class="py-3.5 px-6">ผู้รับเลี้ยง (Sitter)</th>
              <th class="py-3.5 px-6">ช่วงวันที่</th>
              <th class="py-3.5 px-6">ยอดรวม</th>
              <th class="py-3.5 px-6">สถานะงาน</th>
              <th class="py-3.5 px-6 text-right">การชำระเงิน</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="isLoading" class="text-center py-8">
              <td colspan="7" class="py-8 text-slate-400">กำลังโหลดข้อมูล...</td>
            </tr>
            <tr v-else-if="recentBookings.length === 0" class="text-center py-8">
              <td colspan="7" class="py-8 text-slate-400">ยังไม่มีรายการจองในระบบ</td>
            </tr>
            <tr
              v-for="booking in recentBookings"
              :key="booking.id"
              class="hover:bg-slate-50/60 transition-colors"
            >
              <td class="py-4 px-6 font-semibold text-slate-800 font-mono text-xs">
                {{ booking.booking_code }}
              </td>
              <td class="py-4 px-6">
                <div class="flex items-center gap-2.5">
                  <div class="w-7 h-7 rounded-full bg-slate-200 overflow-hidden shrink-0">
                    <img
                      v-if="booking.owner?.avatar_url"
                      :src="booking.owner.avatar_url"
                      class="w-full h-full object-cover"
                    />
                    <div v-else class="w-full h-full flex items-center justify-center text-xs font-bold text-slate-500">
                      {{ booking.owner?.name?.charAt(0) || 'U' }}
                    </div>
                  </div>
                  <span class="font-medium text-slate-800">{{ booking.owner?.name || 'ไม่ระบุ' }}</span>
                </div>
              </td>
              <td class="py-4 px-6">
                <div class="flex items-center gap-2.5">
                  <div class="w-7 h-7 rounded-full bg-slate-200 overflow-hidden shrink-0">
                    <img
                      v-if="booking.sitter?.avatar_url"
                      :src="booking.sitter.avatar_url"
                      class="w-full h-full object-cover"
                    />
                    <div v-else class="w-full h-full flex items-center justify-center text-xs font-bold text-slate-500">
                      {{ booking.sitter?.name?.charAt(0) || 'S' }}
                    </div>
                  </div>
                  <span class="font-medium text-slate-800">{{ booking.sitter?.name || 'ไม่ระบุ' }}</span>
                </div>
              </td>
              <td class="py-4 px-6 text-xs text-slate-500">
                {{ booking.start_date }} ถึง {{ booking.end_date }}
              </td>
              <td class="py-4 px-6 font-semibold text-slate-800">
                {{ formatCurrency(booking.total_price) }}
              </td>
              <td class="py-4 px-6">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border',
                    getStatusBadge(booking.status).class
                  ]"
                >
                  {{ getStatusBadge(booking.status).label }}
                </span>
              </td>
              <td class="py-4 px-6 text-right">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold',
                    booking.payment_status === 'paid' ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-600'
                  ]"
                >
                  {{ booking.payment_status === 'paid' ? 'ชำระแล้ว' : 'ยังไม่ชำระ' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
