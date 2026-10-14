<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api, API_URL } from '../api/client';
import { Search, Eye, X, ExternalLink } from 'lucide-vue-next';

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

function resolveImageUrl(url?: string) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
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

function openDetailModal(booking: any) {
  selectedBooking.value = booking;
  isModalOpen.value = true;
}

async function approveRefund(booking: any) {
  if (!confirm(`ยืนยันว่าได้โอนเงินคืนสำหรับการจอง ${booking.booking_code} ยอด ฿${Number(booking.total_price).toLocaleString()} เรียบร้อยแล้ว?`)) return;
  try {
    await api.post(`/api/admin/bookings/${booking.id}/refund/approve`, {});
    booking.refund_status = 'refunded';
    booking.payment_status = 'refunded';
    booking.status = 'cancelled';
    await loadBookings();
    alert('บันทึกการคืนเงินเรียบร้อยแล้ว');
  } catch (e: any) {
    alert(e.message || 'ดำเนินการไม่สำเร็จ');
  }
}

async function rejectRefund(booking: any) {
  const reason = prompt('ระบุเหตุผลการปฏิเสธการคืนเงิน:');
  if (reason === null) return;
  try {
    await api.post(`/api/admin/bookings/${booking.id}/refund/reject`, { reason });
    booking.refund_status = 'rejected';
    await loadBookings();
    alert('ปฏิเสธคำขอคืนเงินเรียบร้อยแล้ว');
  } catch (e: any) {
    alert(e.message || 'ดำเนินการไม่สำเร็จ');
  }
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

          <!-- Service Address & Emergency Info -->
          <div v-if="selectedBooking.service_address || selectedBooking.emergency_contact_phone || selectedBooking.preferred_vet_clinic" class="bg-slate-50 p-4 rounded-2xl border border-slate-200/60 space-y-2.5 text-xs">
            <div v-if="selectedBooking.service_address">
              <span class="text-slate-400 font-medium">สถานที่นัดหมายบริการ:</span>
              <p class="font-medium text-slate-800 mt-0.5 leading-relaxed">{{ selectedBooking.service_address }}</p>
              <a :href="`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(selectedBooking.service_address)}`" target="_blank" class="inline-flex items-center gap-1 text-[#FF7A59] font-medium mt-1 hover:underline">
                เปิดใน Google Maps <ExternalLink class="w-3 h-3" />
              </a>
            </div>
            <div v-if="selectedBooking.emergency_contact_phone" class="pt-2 border-t border-slate-200 flex justify-between items-center">
              <span class="text-slate-400 font-medium">เบอร์ติดต่อฉุกเฉินสำรอง:</span>
              <span class="font-semibold text-slate-800">{{ selectedBooking.emergency_contact_phone }}</span>
            </div>
            <div v-if="selectedBooking.preferred_vet_clinic" class="pt-2 border-t border-slate-200 flex justify-between items-center">
              <span class="text-slate-400 font-medium">คลินิกสัตว์แพทย์ประจำตัว:</span>
              <span class="font-semibold text-slate-800">{{ selectedBooking.preferred_vet_clinic }}</span>
            </div>
          </div>

          <!-- Refund Status & Admin Actions -->
          <div v-if="selectedBooking.refund_status" class="p-4 rounded-2xl border border-rose-200 bg-rose-50/60 space-y-2 text-xs text-rose-900">
            <span class="font-bold text-rose-950 flex items-center justify-between">
              <span>💸 ข้อมูลการขอคืนเงิน (Refund)</span>
              <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase" :class="selectedBooking.refund_status === 'refunded' ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'">
                {{ selectedBooking.refund_status }}
              </span>
            </span>
            <p v-if="selectedBooking.refund_reason"><strong>เหตุผล:</strong> {{ selectedBooking.refund_reason }}</p>
            <p v-if="selectedBooking.refund_bank_info"><strong>บัญชีรับเงินคืน:</strong> {{ selectedBooking.refund_bank_info }}</p>
            <div v-if="selectedBooking.refund_status === 'requested'" class="flex gap-2 pt-2 border-t border-rose-200">
              <button @click="approveRefund(selectedBooking)" class="px-3.5 py-2 bg-emerald-600 hover:bg-emerald-700 text-white rounded-xl font-semibold cursor-pointer text-xs">
                อนุมัติและบันทึกโอนคืนแล้ว (Approve Refund)
              </button>
              <button @click="rejectRefund(selectedBooking)" class="px-3.5 py-2 bg-rose-100 hover:bg-rose-200 text-rose-800 rounded-xl font-semibold cursor-pointer text-xs">
                ปฏิเสธคำขอคืนเงิน
              </button>
            </div>
          </div>

          <!-- Slip Box if available -->
          <div v-if="selectedBooking.payment_slip_url" class="p-4 rounded-2xl border border-slate-200 bg-slate-50 space-y-2">
            <span class="text-xs font-semibold text-slate-700 block">สลิปการโอนเงินที่แนบมา:</span>
            <div class="flex items-center gap-4">
              <a :href="resolveImageUrl(selectedBooking.payment_slip_url)" target="_blank" class="w-16 h-20 rounded-xl overflow-hidden bg-slate-900 border border-slate-300 block shrink-0 shadow-xs">
                <img :src="resolveImageUrl(selectedBooking.payment_slip_url)" class="w-full h-full object-cover" />
              </a>
              <div class="text-xs space-y-1">
                <p class="text-slate-500">สถานะการชำระ: <span class="font-bold text-slate-800">{{ selectedBooking.payment_status }}</span></p>
                <p v-if="selectedBooking.payment_slip_uploaded_at" class="text-slate-400">แนบเมื่อ: {{ new Date(selectedBooking.payment_slip_uploaded_at).toLocaleString('th-TH') }}</p>
                <a :href="resolveImageUrl(selectedBooking.payment_slip_url)" target="_blank" class="inline-flex items-center gap-1 text-[#FF7A59] font-medium hover:underline">
                  เปิดดูสลิปขนาดเต็ม <ExternalLink class="w-3 h-3" />
                </a>
              </div>
            </div>
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
