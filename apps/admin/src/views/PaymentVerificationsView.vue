<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api, API_URL } from '../api/client';
import {
  CreditCard,
  CheckCircle2,
  XCircle,
  Eye,
  Calendar,
  User,
  ArrowUpRight,
  Clock,
  AlertCircle,
  Search,
  ExternalLink,
  FileCheck2,
  Loader2,
  X
} from 'lucide-vue-next';

interface AdminUserContact {
  name: string;
  email: string;
  phone?: string;
  avatar_url?: string;
}

interface PaymentBookingItem {
  id: string;
  booking_code: string;
  start_date: string;
  end_date: string;
  days_count: number;
  base_price: number;
  service_fee: number;
  total_price: number;
  status: string;
  payment_status: string;
  payment_slip_url?: string;
  payment_slip_uploaded_at?: string;
  payment_rejection_reason?: string;
  special_instructions?: string;
  created_at: string;
  owner?: AdminUserContact;
  sitter?: AdminUserContact;
}

const payments = ref<PaymentBookingItem[]>([]);
const isLoading = ref(true);
const activeTab = ref<'pending_verification' | 'paid' | 'rejected' | 'all'>('pending_verification');
const searchQuery = ref('');

// Slip preview modal
const previewSlipUrl = ref<string | null>(null);

// Approve modal
const isApproveModalOpen = ref(false);
const approvingBooking = ref<PaymentBookingItem | null>(null);

// Reject modal
const isRejectModalOpen = ref(false);
const rejectingBooking = ref<PaymentBookingItem | null>(null);
const rejectReason = ref('');
const isProcessing = ref(false);

const successMessage = ref('');
const errorMessage = ref('');

async function loadPayments() {
  isLoading.value = true;
  try {
    payments.value = await api.get<PaymentBookingItem[]>(`/api/admin/payments?status=${activeTab.value}`);
  } catch (e: any) {
    console.error('Error loading payments:', e);
    errorMessage.value = e.message || 'โหลดรายการสลิปไม่สำเร็จ';
  } finally {
    isLoading.value = false;
  }
}

const filteredPayments = computed(() => {
  if (!searchQuery.value.trim()) return payments.value;
  const q = searchQuery.value.toLowerCase();
  return payments.value.filter(
    (p) =>
      p.booking_code?.toLowerCase().includes(q) ||
      p.owner?.name?.toLowerCase().includes(q) ||
      p.sitter?.name?.toLowerCase().includes(q) ||
      p.owner?.phone?.includes(q)
  );
});

function resolveImageUrl(url?: string) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
}

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('th-TH', { style: 'currency', currency: 'THB', maximumFractionDigits: 0 }).format(amount);
}

function formatDateTime(isoStr?: string) {
  if (!isoStr) return '-';
  const d = new Date(isoStr);
  return d.toLocaleDateString('th-TH', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function openSlipPreview(url?: string) {
  if (!url) return;
  previewSlipUrl.value = resolveImageUrl(url);
}

function openApproveModal(item: PaymentBookingItem) {
  approvingBooking.value = item;
  isApproveModalOpen.value = true;
}

async function confirmApprove() {
  if (!approvingBooking.value) return;

  isProcessing.value = true;
  const item = approvingBooking.value;
  try {
    await api.post(`/api/admin/bookings/${item.id}/payment/approve`);
    item.payment_status = 'paid';
    item.payment_rejection_reason = undefined;
    showSuccess(`อนุมัติการชำระเงิน ${item.booking_code} เรียบร้อยแล้ว`);
    // If currently filtering pending, remove it or reload
    if (activeTab.value === 'pending_verification') {
      payments.value = payments.value.filter((p) => p.id !== item.id);
    }
    isApproveModalOpen.value = false;
    approvingBooking.value = null;
  } catch (e: any) {
    errorMessage.value = 'เกิดข้อผิดพลาด: ' + (e.message || 'ไม่สามารถอนุมัติได้');
  } finally {
    isProcessing.value = false;
  }
}

function openRejectModal(item: PaymentBookingItem) {
  rejectingBooking.value = item;
  rejectReason.value = 'ยอดเงินไม่ตรงกับค่าบริการ หรือสลิปไม่ชัดเจน';
  isRejectModalOpen.value = true;
}

async function confirmReject() {
  if (!rejectingBooking.value) return;

  isProcessing.value = true;
  try {
    await api.post(`/api/admin/bookings/${rejectingBooking.value.id}/payment/reject`, {
      reason: rejectReason.value.trim(),
    });
    rejectingBooking.value.payment_status = 'rejected';
    rejectingBooking.value.payment_rejection_reason = rejectReason.value.trim();
    showSuccess(`ปฏิเสธสลิป ${rejectingBooking.value.booking_code} แล้ว`);
    if (activeTab.value === 'pending_verification') {
      payments.value = payments.value.filter((p) => p.id !== rejectingBooking.value?.id);
    }
    isRejectModalOpen.value = false;
  } catch (e: any) {
    alert('เกิดข้อผิดพลาด: ' + (e.message || 'ไม่สามารถปฏิเสธได้'));
  } finally {
    isProcessing.value = false;
  }
}

function showSuccess(msg: string) {
  successMessage.value = msg;
  setTimeout(() => {
    successMessage.value = '';
  }, 3500);
}

onMounted(() => {
  loadPayments();
});
</script>

<template>
  <div class="space-y-6 max-w-7xl">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800 flex items-center gap-2.5">
          <CreditCard class="w-7 h-7 text-[#FF7A59]" />
          ตรวจสอบการชำระเงิน (Payment Verifications)
        </h1>
        <p class="text-xs text-slate-500 mt-1">
          ตรวจสอบสลิปโอนเงิน PromptPay/ธนาคาร จากเจ้าของแมว และอนุมัติเพื่อปล่อยให้งานเริ่มดูแล
        </p>
      </div>

      <!-- Search box -->
      <div class="relative w-full sm:w-72">
        <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="ค้นหารหัสจอง / ชื่อผู้จอง..."
          class="w-full pl-9 pr-3.5 py-2 rounded-xl border border-slate-200 text-xs focus:outline-hidden focus:border-[#FF7A59] bg-white"
        />
      </div>
    </div>

    <!-- Success Message -->
    <div
      v-if="successMessage"
      class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center gap-3"
    >
      <CheckCircle2 class="w-5 h-5 text-emerald-600 shrink-0" />
      <span>{{ successMessage }}</span>
    </div>

    <!-- Tabs -->
    <div class="flex items-center gap-2 border-b border-slate-200 pb-1 overflow-x-auto">
      <button
        @click="activeTab = 'pending_verification'; loadPayments()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-1.5 whitespace-nowrap cursor-pointer',
          activeTab === 'pending_verification'
            ? 'bg-white text-amber-600 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <Clock class="w-4 h-4 text-amber-500" />
        รอตรวจสอบสลิป
      </button>
      <button
        @click="activeTab = 'paid'; loadPayments()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-1.5 whitespace-nowrap cursor-pointer',
          activeTab === 'paid'
            ? 'bg-white text-emerald-600 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <CheckCircle2 class="w-4 h-4 text-emerald-500" />
        อนุมัติแล้ว (Paid)
      </button>
      <button
        @click="activeTab = 'rejected'; loadPayments()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-1.5 whitespace-nowrap cursor-pointer',
          activeTab === 'rejected'
            ? 'bg-white text-rose-600 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <XCircle class="w-4 h-4 text-rose-500" />
        สลิปถูกปฏิเสธ
      </button>
      <button
        @click="activeTab = 'all'; loadPayments()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all whitespace-nowrap cursor-pointer',
          activeTab === 'all'
            ? 'bg-white text-slate-800 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        ทั้งหมดที่มีสลิป
      </button>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="i in 3" :key="i" class="bg-white rounded-2xl border border-slate-100 p-5 animate-pulse h-28"></div>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="filteredPayments.length === 0"
      class="bg-white rounded-2xl border border-slate-200/80 p-12 text-center"
    >
      <div class="w-16 h-16 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center mx-auto mb-4">
        <FileCheck2 class="w-8 h-8" />
      </div>
      <h3 class="text-base font-semibold text-slate-800">ไม่พบรายการสลิปในหมวดนี้</h3>
      <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
        เมื่อลูกค้าทำการโอนเงินและแนบสลิปผ่านหน้าเว็บ รายการสลิปจะเข้ามาปรากฏที่นี่เพื่อให้คุณตรวจสอบความถูกต้อง
      </p>
    </div>

    <!-- Payments List -->
    <div v-else class="space-y-4">
      <div
        v-for="item in filteredPayments"
        :key="item.id"
        class="bg-white rounded-2xl border border-slate-200 shadow-xs hover:shadow-md transition-all p-5 flex flex-col lg:flex-row items-start lg:items-center justify-between gap-5"
      >
        <!-- Left: Slip Thumbnail + Booking Info -->
        <div class="flex items-start gap-4 min-w-0">
          <!-- Slip Thumbnail -->
          <div
            v-if="item.payment_slip_url"
            @click="openSlipPreview(item.payment_slip_url)"
            class="relative w-20 h-24 rounded-xl overflow-hidden bg-slate-900 border border-slate-200 shrink-0 cursor-pointer group shadow-xs hover:border-[#FF7A59]"
            title="คลิกเพื่อดูสลิปขนาดเต็ม"
          >
            <img
              :src="resolveImageUrl(item.payment_slip_url)"
              alt="Slip"
              class="w-full h-full object-cover group-hover:scale-105 transition-transform"
            />
            <div class="absolute inset-0 bg-black/30 group-hover:bg-black/10 flex items-center justify-center text-white transition-colors">
              <Eye class="w-5 h-5 drop-shadow-md" />
            </div>
          </div>
          <div
            v-else
            class="w-20 h-24 rounded-xl bg-slate-100 border border-slate-200 flex flex-col items-center justify-center text-slate-400 shrink-0 text-[11px]"
          >
            <AlertCircle class="w-5 h-5 mb-1" />
            ไม่มีสลิป
          </div>

          <!-- Booking Details -->
          <div class="space-y-1 min-w-0">
            <div class="flex items-center gap-2">
              <span class="text-xs font-mono font-bold text-slate-800 bg-slate-100 px-2.5 py-0.5 rounded-md">
                {{ item.booking_code }}
              </span>

              <!-- Status Badge -->
              <span
                v-if="item.payment_status === 'pending_verification'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200 flex items-center gap-1"
              >
                <Clock class="w-3 h-3 text-amber-500" />
                รอตรวจสอบ
              </span>
              <span
                v-else-if="item.payment_status === 'paid'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 flex items-center gap-1"
              >
                <CheckCircle2 class="w-3 h-3 text-emerald-500" />
                ชำระแล้ว
              </span>
              <span
                v-else-if="item.payment_status === 'rejected'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200 flex items-center gap-1"
              >
                <XCircle class="w-3 h-3 text-rose-500" />
                ปฏิเสธแล้ว
              </span>
            </div>

            <!-- Parties -->
            <p class="text-sm font-semibold text-slate-800">
              ผู้จอง: {{ item.owner?.name ?? 'ไม่ระบุ' }}
              <span class="text-xs font-normal text-slate-500">({{ item.owner?.phone ?? item.owner?.email }})</span>
            </p>
            <p class="text-xs text-slate-500">
              พี่เลี้ยง: {{ item.sitter?.name ?? 'ไม่ระบุ' }} · ระยะเวลา: {{ item.days_count }} วัน ({{ item.start_date }} ถึง {{ item.end_date }})
            </p>

            <p class="text-[11px] text-slate-400 flex items-center gap-1 mt-1">
              <Clock class="w-3 h-3" />
              แนบสลิปเมื่อ: {{ formatDateTime(item.payment_slip_uploaded_at) }}
            </p>

            <!-- Rejection reason if any -->
            <p v-if="item.payment_rejection_reason" class="text-xs text-rose-600 bg-rose-50/70 rounded-lg p-2 mt-1">
              <strong>เหตุผลที่ปฏิเสธ:</strong> {{ item.payment_rejection_reason }}
            </p>
          </div>
        </div>

        <!-- Right: Amount & Actions -->
        <div class="w-full lg:w-auto flex flex-row lg:flex-col items-center lg:items-end justify-between lg:justify-center gap-3 pt-3 lg:pt-0 border-t lg:border-t-0 border-slate-100">
          <div class="text-left lg:text-right">
            <span class="text-xs text-slate-400 block">ยอดรวมที่ต้องชำระ</span>
            <span class="text-xl font-bold text-[#FF7A59]">{{ formatCurrency(item.total_price) }}</span>
            <span class="text-[11px] text-slate-400 block">(ค่าธรรมเนียม: {{ formatCurrency(item.service_fee) }})</span>
          </div>

          <!-- Buttons -->
          <div class="flex items-center gap-2">
            <template v-if="item.payment_status === 'pending_verification'">
              <button
                @click="openRejectModal(item)"
                class="px-3.5 py-2 rounded-xl border border-rose-200 bg-rose-50 hover:bg-rose-100 text-rose-700 text-xs font-semibold transition-colors cursor-pointer"
              >
                ปฏิเสธสลิป
              </button>
              <button
                id="btn-open-approve"
                @click="openApproveModal(item)"
                class="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-semibold shadow-xs transition-colors cursor-pointer flex items-center gap-1.5"
              >
                <CheckCircle2 class="w-3.5 h-3.5" />
                อนุมัติการชำระเงิน
              </button>
            </template>

            <button
              v-else-if="item.payment_slip_url"
              @click="openSlipPreview(item.payment_slip_url)"
              class="px-3 py-1.5 rounded-xl border border-slate-200 hover:bg-slate-50 text-slate-600 text-xs font-medium flex items-center gap-1 cursor-pointer"
            >
              <Eye class="w-3.5 h-3.5" />
              ดูรูปสลิป
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Slip Image Preview Modal -->
    <div
      v-if="previewSlipUrl"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-xs"
      @click="previewSlipUrl = null"
    >
      <div class="relative max-w-lg w-full max-h-[90vh] flex flex-col items-center">
        <button
          @click="previewSlipUrl = null"
          class="absolute -top-10 right-0 text-white hover:text-slate-300 p-1"
        >
          <X class="w-6 h-6" />
        </button>
        <img
          :src="previewSlipUrl"
          alt="Full Slip"
          class="max-w-full max-h-[85vh] rounded-2xl shadow-2xl object-contain bg-white"
          @click.stop
        />
      </div>
    </div>

    <!-- Approve Confirmation Modal -->
    <div
      v-if="isApproveModalOpen && approvingBooking"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs"
    >
      <div class="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
        <div class="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center mb-4">
          <CheckCircle2 class="w-6 h-6" />
        </div>
        <h3 class="text-base font-bold text-slate-800 mb-1">ยืนยันการอนุมัติการชำระเงิน</h3>
        <p class="text-xs text-slate-500 mb-4">
          การอนุมัติจะเปลี่ยนสถานะการจองเป็น "ชำระเงินแล้ว" และแจ้งเตือนให้เจ้าของและพี่เลี้ยงทราบ
        </p>

        <div class="bg-slate-50 rounded-xl p-3.5 space-y-2 text-xs mb-5 border border-slate-100">
          <div class="flex justify-between text-slate-600">
            <span>รหัสการจอง:</span>
            <span class="font-mono font-bold text-slate-800">{{ approvingBooking.booking_code }}</span>
          </div>
          <div class="flex justify-between text-slate-600">
            <span>ผู้จ้าง (เจ้าของแมว):</span>
            <span class="font-semibold text-slate-800">{{ approvingBooking.owner?.name }}</span>
          </div>
          <div class="flex justify-between text-slate-600">
            <span>ยอดชำระที่ตรวจสอบ:</span>
            <span class="font-bold text-emerald-600 text-sm">{{ formatCurrency(approvingBooking.total_price) }}</span>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2">
          <button
            type="button"
            @click="isApproveModalOpen = false; approvingBooking = null"
            class="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 hover:bg-slate-50 text-xs font-semibold cursor-pointer"
          >
            ยกเลิก
          </button>
          <button
            type="button"
            id="btn-confirm-approve"
            @click="confirmApprove"
            :disabled="isProcessing"
            class="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold transition-colors disabled:opacity-50 cursor-pointer flex items-center gap-1.5"
          >
            <span v-if="isProcessing" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>ยืนยันอนุมัติ</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Reject Reason Modal -->
    <div
      v-if="isRejectModalOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs"
    >
      <div class="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
        <h3 class="text-base font-bold text-slate-800 mb-2">ระบุเหตุผลการปฏิเสธสลิป</h3>
        <p class="text-xs text-slate-500 mb-4">
          ข้อความนี้จะถูกส่งแจ้งเตือนไปยังเจ้าของแมว เพื่อให้เขาแก้ไขและแนบสลิปที่ถูกต้องเข้ามาใหม่
        </p>

        <textarea
          v-model="rejectReason"
          rows="3"
          placeholder="เช่น สลิปไม่ชัดเจน ยอดเงินไม่ตรง หรือสลิปซ้ำ..."
          class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-hidden focus:border-rose-500 focus:ring-2 focus:ring-rose-200 mb-4"
        ></textarea>

        <div class="flex items-center justify-end gap-2">
          <button
            @click="isRejectModalOpen = false"
            class="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 hover:bg-slate-50 text-xs font-semibold"
          >
            ยกเลิก
          </button>
          <button
            @click="confirmReject"
            :disabled="isProcessing"
            class="px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold transition-colors disabled:opacity-50"
          >
            ยืนยันปฏิเสธสลิป
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
