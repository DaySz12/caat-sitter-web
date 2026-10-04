<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import { Search, Eye, X } from 'lucide-vue-next';

const bookings = ref<any[]>([]);
const isLoading = ref(true);
const searchQuery = ref('');
const statusFilter = ref('all');
const selectedBooking = ref<any | null>(null);
const isModalOpen = ref(false);

async function loadBookings() {
  isLoading.value = true;
  try {
    bookings.value = await api.get<any[]>(`/api/admin/bookings?status=${statusFilter.value}`);
  } catch (e) {
    console.error('Error loading bookings:', e);
  } finally {
    isLoading.value = false;
  }
}

const filteredBookings = computed(() => {
  if (!searchQuery.value.trim()) return bookings.value;
  const q = searchQuery.value.toLowerCase();
  return bookings.value.filter(
    b =>
      b.booking_code?.toLowerCase().includes(q) ||
      b.owner?.name?.toLowerCase().includes(q) ||
      b.sitter?.name?.toLowerCase().includes(q)
  );
});

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

function openDetailModal(booking: any) {
  selectedBooking.value = booking;
  isModalOpen.value = true;
}

onMounted(() => {
  loadBookings();
});
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">จัดการรายการจอง (Booking Management)</h1>
        <p class="text-xs text-slate-500 mt-1">รายการจองทั้งหมดบนระบบ ติดตามสถานะงานและยอดชำระเงิน</p>
      </div>

      <!-- Search & Filters -->
      <div class="flex flex-wrap items-center gap-3">
        <div class="relative">
          <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="ค้นหารหัสจอง หรือชื่อ..."
            class="pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 bg-white text-xs w-56 focus:outline-none focus:ring-2 focus:ring-[#FF7A59]"
          />
        </div>

        <select
          v-model="statusFilter"
          @change="loadBookings"
          class="px-3.5 py-2.5 rounded-xl border border-slate-200 bg-white text-xs text-slate-700 font-semibold focus:outline-none focus:ring-2 focus:ring-[#FF7A59] cursor-pointer"
        >
          <option value="all">ทุกสถานะ</option>
          <option value="pending">รอยืนยัน (Pending)</option>
          <option value="confirmed">ยืนยันแล้ว (Confirmed)</option>
          <option value="in_progress">กำลังดูแล (In Progress)</option>
          <option value="completed">เสร็จสิ้น (Completed)</option>
          <option value="cancelled">ยกเลิก (Cancelled)</option>
        </select>
      </div>
    </div>

    <!-- Table -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-xs overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-slate-600">
          <thead class="bg-slate-50 text-xs uppercase font-semibold text-slate-500 border-b border-slate-100">
            <tr>
              <th class="py-3.5 px-6">รหัสการจอง</th>
              <th class="py-3.5 px-6">เจ้าของแมว (Owner)</th>
              <th class="py-3.5 px-6">ผู้รับเลี้ยง (Sitter)</th>
              <th class="py-3.5 px-6">วันที่</th>
              <th class="py-3.5 px-6">จำนวนวัน</th>
              <th class="py-3.5 px-6">ยอดรวม</th>
              <th class="py-3.5 px-6">สถานะ</th>
              <th class="py-3.5 px-6 text-right">รายละเอียด</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="isLoading" class="text-center py-8">
              <td colspan="8" class="py-8 text-slate-400">กำลังโหลดรายการจอง...</td>
            </tr>
            <tr v-else-if="filteredBookings.length === 0" class="text-center py-8">
              <td colspan="8" class="py-8 text-slate-400">ไม่พบรายการจองที่ตรงกับเงื่อนไข</td>
            </tr>
            <tr v-for="b in filteredBookings" :key="b.id" class="hover:bg-slate-50/60 transition-colors">
              <td class="py-4 px-6 font-mono text-xs font-bold text-slate-800">
                {{ b.booking_code }}
              </td>
              <td class="py-4 px-6 font-medium text-slate-800">
                {{ b.owner?.name || 'ไม่ระบุ' }}
              </td>
              <td class="py-4 px-6 font-medium text-slate-800">
                {{ b.sitter?.name || 'ไม่ระบุ' }}
              </td>
              <td class="py-4 px-6 text-xs text-slate-500">
                {{ b.start_date }} ถึง {{ b.end_date }}
              </td>
              <td class="py-4 px-6 text-xs text-slate-600">
                {{ b.days_count }} วัน
              </td>
              <td class="py-4 px-6 font-bold text-slate-800">
                ฿{{ Number(b.total_price).toLocaleString() }}
              </td>
              <td class="py-4 px-6">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold border',
                    getStatusBadge(b.status).class
                  ]"
                >
                  {{ getStatusBadge(b.status).label }}
                </span>
              </td>
              <td class="py-4 px-6 text-right">
                <button
                  @click="openDetailModal(b)"
                  class="p-2 rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-600 transition-colors cursor-pointer"
                >
                  <Eye class="w-4 h-4" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Booking Detail Modal -->
    <div
      v-if="isModalOpen && selectedBooking"
      class="fixed inset-0 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 z-50 overflow-y-auto"
    >
      <div class="bg-white rounded-3xl max-w-xl w-full p-6 shadow-2xl border border-slate-100 space-y-5">
        <div class="flex items-center justify-between border-b border-slate-100 pb-4">
          <div>
            <span class="text-xs text-slate-400 font-mono font-bold">{{ selectedBooking.booking_code }}</span>
            <h3 class="font-bold text-slate-800 text-base mt-0.5">รายละเอียดรายการจอง</h3>
          </div>
          <button @click="isModalOpen = false" class="text-slate-400 hover:text-slate-600 p-1 cursor-pointer">
            <X class="w-5 h-5" />
          </button>
        </div>

        <div class="space-y-4 text-sm text-slate-700">
          <div class="grid grid-cols-2 gap-4 bg-slate-50 p-4 rounded-2xl border border-slate-200/60">
            <div>
              <span class="text-xs text-slate-400">เจ้าของแมว:</span>
              <p class="font-semibold text-slate-800">{{ selectedBooking.owner?.name }} ({{ selectedBooking.owner?.phone || '-' }})</p>
            </div>
            <div>
              <span class="text-xs text-slate-400">ผู้รับเลี้ยง:</span>
              <p class="font-semibold text-slate-800">{{ selectedBooking.sitter?.name }} ({{ selectedBooking.sitter?.phone || '-' }})</p>
            </div>
            <div>
              <span class="text-xs text-slate-400">ช่วงวันที่จอง:</span>
              <p class="font-medium text-slate-800">{{ selectedBooking.start_date }} ถึง {{ selectedBooking.end_date }} ({{ selectedBooking.days_count }} วัน)</p>
            </div>
            <div>
              <span class="text-xs text-slate-400">สถานะการชำระ:</span>
              <p class="font-semibold text-emerald-600">{{ selectedBooking.payment_status === 'paid' ? 'ชำระแล้ว' : 'ยังไม่ชำระ' }}</p>
            </div>
          </div>

          <!-- Pricing Breakdown -->
          <div class="p-4 rounded-2xl bg-amber-50/60 border border-amber-200/60 space-y-1.5">
            <div class="flex justify-between text-xs text-slate-600">
              <span>ค่าบริการรับเลี้ยง:</span>
              <span>฿{{ Number(selectedBooking.base_price || selectedBooking.total_price).toLocaleString() }}</span>
            </div>
            <div class="flex justify-between text-xs text-slate-600">
              <span>ค่าธรรมเนียมแพลตฟอร์ม:</span>
              <span>฿{{ Number(selectedBooking.service_fee || 0).toLocaleString() }}</span>
            </div>
            <div class="flex justify-between text-sm font-bold text-slate-800 pt-2 border-t border-amber-200">
              <span>ยอดรวมทั้งหมด:</span>
              <span class="text-[#FF7A59]">฿{{ Number(selectedBooking.total_price).toLocaleString() }}</span>
            </div>
          </div>

          <!-- Special Note -->
          <div v-if="selectedBooking.special_instructions">
            <span class="text-xs text-slate-400 font-medium">คำขอพิเศษเพิ่มเติม:</span>
            <p class="text-xs bg-slate-50 p-3 rounded-xl border border-slate-200 mt-1 text-slate-600">
              {{ selectedBooking.special_instructions }}
            </p>
          </div>
        </div>

        <div class="flex justify-end pt-2">
          <button @click="isModalOpen = false" class="px-5 py-2.5 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 font-semibold text-xs transition-colors cursor-pointer">
            ปิดหน้าต่าง
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
