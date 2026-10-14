<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { api, API_URL } from '../api/client';
import {
  Banknote,
  Search,
  CheckCircle2,
  XCircle,
  Clock,
  Eye,
  AlertCircle,
  Upload,
  X,
  FileCheck2,
  Copy,
  Check,
  Building2
} from 'lucide-vue-next';

interface PayoutItem {
  id: string;
  sitter_id: string;
  amount: number;
  bank_name: string;
  bank_account_number: string;
  bank_account_name: string;
  promptpay_number?: string;
  status: 'pending' | 'completed' | 'rejected';
  admin_notes?: string;
  transfer_slip_url?: string;
  created_at: string;
  processed_at?: string;
  sitter?: {
    name: string;
    email: string;
    phone?: string;
    avatar_url?: string;
  };
}

const payouts = ref<PayoutItem[]>([]);
const isLoading = ref(true);
const activeTab = ref<'pending' | 'completed' | 'rejected' | 'all'>('pending');
const searchQuery = ref('');

// Modals
const isApproveModalOpen = ref(false);
const approvingPayout = ref<PayoutItem | null>(null);
const transferSlipFile = ref<File | null>(null);
const transferSlipPreview = ref<string | null>(null);

const isRejectModalOpen = ref(false);
const rejectingPayout = ref<PayoutItem | null>(null);
const rejectReason = ref('');

const previewSlipUrl = ref<string | null>(null);
const isProcessing = ref(false);
const successMessage = ref('');
const errorMessage = ref('');
const copiedField = ref<string | null>(null);

async function loadPayouts() {
  isLoading.value = true;
  try {
    payouts.value = await api.get<PayoutItem[]>(`/api/admin/payouts?status=${activeTab.value}`);
  } catch (e: any) {
    console.error('Error loading payouts:', e);
    errorMessage.value = e.message || 'โหลดรายการถอนเงินไม่สำเร็จ';
  } finally {
    isLoading.value = false;
  }
}

const filteredPayouts = computed(() => {
  if (!searchQuery.value.trim()) return payouts.value;
  const q = searchQuery.value.toLowerCase();
  return payouts.value.filter(
    (p) =>
      p.sitter?.name?.toLowerCase().includes(q) ||
      p.bank_account_number?.includes(q) ||
      p.bank_name?.toLowerCase().includes(q) ||
      p.promptpay_number?.includes(q) ||
      p.sitter?.email?.toLowerCase().includes(q)
  );
});

const pendingTotalAmount = computed(() => {
  return payouts.value
    .filter((p) => p.status === 'pending')
    .reduce((sum, p) => sum + Number(p.amount), 0);
});

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

function resolveImageUrl(url?: string) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
}

function copyText(text: string, fieldKey: string) {
  navigator.clipboard.writeText(text);
  copiedField.value = fieldKey;
  setTimeout(() => {
    if (copiedField.value === fieldKey) copiedField.value = null;
  }, 2000);
}

function openApproveModal(item: PayoutItem) {
  approvingPayout.value = item;
  transferSlipFile.value = null;
  transferSlipPreview.value = null;
  isApproveModalOpen.value = true;
}

function onSlipFileChange(e: Event) {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;
  transferSlipFile.value = file;
  transferSlipPreview.value = URL.createObjectURL(file);
}

async function confirmApprove() {
  if (!approvingPayout.value) return;
  isProcessing.value = true;
  const item = approvingPayout.value;

  try {
    let slipUrl: string | undefined = undefined;
    if (transferSlipFile.value) {
      const res = await api.upload('payout-slips', transferSlipFile.value);
      slipUrl = res.url;
    }

    await api.post(`/api/admin/payouts/${item.id}/approve`, { transferSlipUrl: slipUrl });
    item.status = 'completed';
    item.transfer_slip_url = slipUrl;
    showSuccess(`บันทึกการโอนเงินจำนวน ${formatCurrency(item.amount)} สำเร็จ`);

    if (activeTab.value === 'pending') {
      payouts.value = payouts.value.filter((p) => p.id !== item.id);
    }
    isApproveModalOpen.value = false;
    approvingPayout.value = null;
  } catch (e: any) {
    errorMessage.value = e.message || 'ไม่สามารถอนุมัติการถอนเงินได้';
  } finally {
    isProcessing.value = false;
  }
}

function openRejectModal(item: PayoutItem) {
  rejectingPayout.value = item;
  rejectReason.value = 'เลขบัญชีธนาคารไม่ถูกต้อง หรือชื่อบัญชีไม่ตรงกับชื่อพี่เลี้ยง';
  isRejectModalOpen.value = true;
}

async function confirmReject() {
  if (!rejectingPayout.value) return;
  isProcessing.value = true;
  const item = rejectingPayout.value;

  try {
    await api.post(`/api/admin/payouts/${item.id}/reject`, { reason: rejectReason.value.trim() });
    item.status = 'rejected';
    item.admin_notes = rejectReason.value.trim();
    showSuccess(`ปฏิเสธคำขอและคืนยอดเงิน ${formatCurrency(item.amount)} เข้ากระเป๋าพี่เลี้ยงแล้ว`);

    if (activeTab.value === 'pending') {
      payouts.value = payouts.value.filter((p) => p.id !== item.id);
    }
    isRejectModalOpen.value = false;
    rejectingPayout.value = null;
  } catch (e: any) {
    errorMessage.value = e.message || 'ไม่สามารถปฏิเสธคำขอได้';
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

onMounted(loadPayouts);
</script>

<template>
  <div class="space-y-6 max-w-7xl">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800 flex items-center gap-2">
          <Banknote class="w-7 h-7 text-[#FF7A59]" />
          คำขอถอนเงินของพี่เลี้ยง (Payouts)
        </h1>
        <p class="text-xs text-slate-500 mt-1">
          ตรวจสอบคำขอถอนเงินและโอนค่าบริการให้แก่พี่เลี้ยงแมวเมื่อเสร็จสิ้นงาน
        </p>
      </div>

      <div class="flex items-center gap-3">
        <div class="relative w-full sm:w-72">
          <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="ค้นหาชื่อพี่เลี้ยง, เลขบัญชี..."
            class="w-full pl-9 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-hidden focus:border-[#FF7A59] bg-white shadow-xs"
          />
        </div>
      </div>
    </div>

    <!-- Toast Notifications -->
    <div
      v-if="successMessage"
      class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center justify-between shadow-xs transition-all animate-fade-in"
    >
      <div class="flex items-center gap-2.5">
        <CheckCircle2 class="w-5 h-5 text-emerald-600" />
        <span class="font-medium">{{ successMessage }}</span>
      </div>
      <button @click="successMessage = ''" class="text-emerald-500 hover:text-emerald-800"><X class="w-4 h-4" /></button>
    </div>

    <div
      v-if="errorMessage"
      class="p-4 rounded-xl bg-rose-50 border border-rose-200 text-sm text-rose-700 flex items-center justify-between shadow-xs transition-all animate-fade-in"
    >
      <div class="flex items-center gap-2.5">
        <AlertCircle class="w-5 h-5 text-rose-600" />
        <span class="font-medium">{{ errorMessage }}</span>
      </div>
      <button @click="errorMessage = ''" class="text-rose-500 hover:text-rose-800"><X class="w-4 h-4" /></button>
    </div>

    <!-- Tabs Navigation -->
    <div class="flex items-center gap-2 border-b border-slate-200 pb-3 overflow-x-auto">
      <button
        @click="activeTab = 'pending'; loadPayouts()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-2 whitespace-nowrap cursor-pointer',
          activeTab === 'pending'
            ? 'bg-white text-amber-700 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <Clock class="w-4 h-4 text-amber-500" />
        รอโอนเงิน
        <span
          v-if="payouts.filter(p => p.status === 'pending').length > 0"
          class="ml-1 px-2 py-0.5 text-xs bg-amber-100 text-amber-800 rounded-full font-bold"
        >
          {{ payouts.filter(p => p.status === 'pending').length }}
        </span>
      </button>

      <button
        @click="activeTab = 'completed'; loadPayouts()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-2 whitespace-nowrap cursor-pointer',
          activeTab === 'completed'
            ? 'bg-white text-emerald-700 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <CheckCircle2 class="w-4 h-4 text-emerald-500" />
        โอนสำเร็จแล้ว
      </button>

      <button
        @click="activeTab = 'rejected'; loadPayouts()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-2 whitespace-nowrap cursor-pointer',
          activeTab === 'rejected'
            ? 'bg-white text-rose-600 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <XCircle class="w-4 h-4 text-rose-500" />
        คำขอถูกปฏิเสธ
      </button>

      <button
        @click="activeTab = 'all'; loadPayouts()"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all whitespace-nowrap cursor-pointer',
          activeTab === 'all'
            ? 'bg-white text-slate-800 shadow-xs border border-slate-200 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        ทั้งหมด
      </button>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="isLoading" class="space-y-4">
      <div v-for="i in 3" :key="i" class="bg-white rounded-2xl border border-slate-100 p-5 animate-pulse h-28"></div>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="filteredPayouts.length === 0"
      class="bg-white rounded-2xl border border-slate-200/80 p-12 text-center"
    >
      <div class="w-16 h-16 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center mx-auto mb-4">
        <FileCheck2 class="w-8 h-8" />
      </div>
      <h3 class="text-base font-semibold text-slate-800">ไม่พบรายการคำขอถอนเงินในหมวดนี้</h3>
      <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
        เมื่อพี่เลี้ยงทำรายการขอถอนเงินจากหน้าเว็บ รายการจะปรากฏที่นี่เพื่อให้คุณตรวจสอบข้อมูลบัญชีและทำการโอนเงิน
      </p>
    </div>

    <!-- Payouts List -->
    <div v-else class="space-y-4">
      <div
        v-for="item in filteredPayouts"
        :key="item.id"
        class="bg-white rounded-2xl border border-slate-200 shadow-xs hover:shadow-md transition-all p-5 flex flex-col lg:flex-row items-start lg:items-center justify-between gap-5"
      >
        <!-- Left: Sitter & Bank Info -->
        <div class="flex items-start gap-4 min-w-0">
          <!-- Sitter Avatar -->
          <div class="w-14 h-14 rounded-2xl bg-orange-50 border border-orange-100 flex items-center justify-center text-orange-600 font-bold text-lg overflow-hidden shrink-0 shadow-xs">
            <img
              v-if="item.sitter?.avatar_url"
              :src="resolveImageUrl(item.sitter.avatar_url)"
              alt="Avatar"
              class="w-full h-full object-cover"
            />
            <span v-else>{{ item.sitter?.name?.charAt(0) || 'S' }}</span>
          </div>

          <!-- Account Details -->
          <div class="space-y-1.5 min-w-0">
            <div class="flex items-center gap-2.5">
              <span class="font-bold text-slate-900 text-sm truncate">{{ item.sitter?.name }}</span>
              <span class="text-xs text-slate-400">({{ item.sitter?.email }})</span>

              <!-- Status Badge -->
              <span
                v-if="item.status === 'pending'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200 flex items-center gap-1"
              >
                <Clock class="w-3 h-3 text-amber-500" />
                รอโอนเงิน
              </span>
              <span
                v-else-if="item.status === 'completed'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 flex items-center gap-1"
              >
                <CheckCircle2 class="w-3 h-3 text-emerald-500" />
                โอนสำเร็จแล้ว
              </span>
              <span
                v-else-if="item.status === 'rejected'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200 flex items-center gap-1"
              >
                <XCircle class="w-3 h-3 text-rose-500" />
                ถูกปฏิเสธ
              </span>
            </div>

            <!-- Bank Transfer Details Card -->
            <div class="bg-slate-50 border border-slate-200/80 rounded-xl p-3 text-xs space-y-1 max-w-md">
              <div class="flex items-center justify-between text-slate-600">
                <span class="flex items-center gap-1"><Building2 class="w-3.5 h-3.5 text-slate-400" /> ธนาคาร:</span>
                <span class="font-semibold text-slate-800">{{ item.bank_name }}</span>
              </div>
              <div class="flex items-center justify-between text-slate-600">
                <span>เลขที่บัญชี:</span>
                <div class="flex items-center gap-1.5">
                  <span class="font-mono font-bold text-slate-900 text-sm">{{ item.bank_account_number }}</span>
                  <button
                    type="button"
                    @click="copyText(item.bank_account_number, `acc-${item.id}`)"
                    class="p-1 rounded bg-white border border-slate-200 text-slate-500 hover:text-slate-800 cursor-pointer"
                    title="คัดลอกเลขบัญชี"
                  >
                    <Check v-if="copiedField === `acc-${item.id}`" class="w-3 h-3 text-green-600" />
                    <Copy v-else class="w-3 h-3" />
                  </button>
                </div>
              </div>
              <div class="flex items-center justify-between text-slate-600">
                <span>ชื่อบัญชี:</span>
                <span class="font-medium text-slate-800">{{ item.bank_account_name }}</span>
              </div>
              <div v-if="item.promptpay_number" class="flex items-center justify-between text-slate-600">
                <span>พร้อมเพย์:</span>
                <div class="flex items-center gap-1.5">
                  <span class="font-mono font-medium text-slate-800">{{ item.promptpay_number }}</span>
                  <button
                    type="button"
                    @click="copyText(item.promptpay_number, `pp-${item.id}`)"
                    class="p-1 rounded bg-white border border-slate-200 text-slate-500 hover:text-slate-800 cursor-pointer"
                    title="คัดลอกพร้อมเพย์"
                  >
                    <Check v-if="copiedField === `pp-${item.id}`" class="w-3 h-3 text-green-600" />
                    <Copy v-else class="w-3 h-3" />
                  </button>
                </div>
              </div>
            </div>

            <p class="text-[11px] text-slate-400 flex items-center gap-1 pt-0.5">
              <Clock class="w-3 h-3" /> ยื่นคำขอเมื่อ: {{ formatDateTime(item.created_at) }}
              <span v-if="item.processed_at">· ดำเนินการเมื่อ: {{ formatDateTime(item.processed_at) }}</span>
            </p>

            <p v-if="item.admin_notes" class="text-xs text-rose-600 bg-rose-50/70 rounded-lg p-2 mt-1">
              <strong>เหตุผลที่ปฏิเสธ:</strong> {{ item.admin_notes }}
            </p>
          </div>
        </div>

        <!-- Right: Amount & Actions -->
        <div class="w-full lg:w-auto flex flex-row lg:flex-col items-center lg:items-end justify-between lg:justify-center gap-3 pt-3 lg:pt-0 border-t lg:border-t-0 border-slate-100">
          <div class="text-left lg:text-right">
            <span class="text-xs text-slate-400 block">ยอดเงินที่ขอถอน</span>
            <span class="text-2xl font-black text-emerald-600">{{ formatCurrency(item.amount) }}</span>
          </div>

          <div class="flex items-center gap-2">
            <template v-if="item.status === 'pending'">
              <button
                type="button"
                id="btn-open-reject-payout"
                @click="openRejectModal(item)"
                class="px-3.5 py-2 rounded-xl border border-rose-200 bg-rose-50 hover:bg-rose-100 text-rose-700 text-xs font-semibold transition-colors cursor-pointer"
              >
                ปฏิเสธคำขอ
              </button>
              <button
                type="button"
                id="btn-open-approve-payout"
                @click="openApproveModal(item)"
                class="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold shadow-xs transition-colors cursor-pointer flex items-center gap-1.5"
              >
                <CheckCircle2 class="w-3.5 h-3.5" />
                บันทึกการโอนเงิน
              </button>
            </template>

            <button
              v-else-if="item.transfer_slip_url"
              type="button"
              @click="previewSlipUrl = resolveImageUrl(item.transfer_slip_url)"
              class="px-3 py-1.5 rounded-xl border border-slate-200 hover:bg-slate-50 text-slate-600 text-xs font-medium flex items-center gap-1 cursor-pointer"
            >
              <Eye class="w-3.5 h-3.5" /> ดูสลิปหลักฐาน
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Approve Modal -->
    <div
      v-if="isApproveModalOpen && approvingPayout"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs"
    >
      <div class="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
        <div class="w-12 h-12 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center mb-4">
          <CheckCircle2 class="w-6 h-6" />
        </div>
        <h3 class="text-base font-bold text-slate-800 mb-1">ยืนยันการโอนเงินให้พี่เลี้ยง</h3>
        <p class="text-xs text-slate-500 mb-4">
          กรุณาโอนเงินเข้าบัญชีพี่เลี้ยงตามข้อมูลด้านล่างให้เรียบร้อย จากนั้นสามารถแนบสลิปเพื่อยืนยัน
        </p>

        <div class="bg-slate-50 rounded-xl p-3.5 space-y-2 text-xs mb-4 border border-slate-100">
          <div class="flex justify-between text-slate-600">
            <span>พี่เลี้ยง:</span>
            <span class="font-bold text-slate-800">{{ approvingPayout.sitter?.name }}</span>
          </div>
          <div class="flex justify-between text-slate-600">
            <span>ธนาคาร:</span>
            <span class="font-semibold text-slate-800">{{ approvingPayout.bank_name }}</span>
          </div>
          <div class="flex justify-between text-slate-600">
            <span>เลขที่บัญชี:</span>
            <span class="font-mono font-bold text-slate-900 text-sm">{{ approvingPayout.bank_account_number }}</span>
          </div>
          <div class="flex justify-between text-slate-600">
            <span>ชื่อบัญชี:</span>
            <span class="font-medium text-slate-800">{{ approvingPayout.bank_account_name }}</span>
          </div>
          <div class="flex justify-between text-slate-600 pt-1 border-t border-slate-200">
            <span>ยอดที่ต้องโอน:</span>
            <span class="font-black text-emerald-600 text-base">{{ formatCurrency(approvingPayout.amount) }}</span>
          </div>
        </div>

        <!-- Optional Transfer Slip Upload -->
        <div class="mb-5">
          <label class="block text-xs font-semibold text-slate-700 mb-1.5">แนบสลิปหลักฐานการโอน (ไม่บังคับ)</label>
          <div
            v-if="!transferSlipFile"
            class="border-2 border-dashed border-slate-200 hover:border-emerald-500 rounded-xl p-4 text-center cursor-pointer transition bg-slate-50/50"
            @click="($refs.slipInput as HTMLInputElement)?.click()"
          >
            <input
              ref="slipInput"
              type="file"
              accept="image/*"
              class="hidden"
              @change="onSlipFileChange"
            />
            <Upload class="w-6 h-6 text-slate-400 mx-auto mb-1" />
            <p class="text-xs font-medium text-slate-600">คลิกเพื่อแนบสลิปการโอน</p>
          </div>
          <div v-else class="flex items-center gap-3 p-2 bg-slate-50 rounded-xl border border-slate-200">
            <img :src="transferSlipPreview!" alt="Slip" class="w-12 h-12 rounded-lg object-cover" />
            <span class="text-xs text-slate-700 truncate flex-1">{{ transferSlipFile.name }}</span>
            <button type="button" @click="transferSlipFile = null; transferSlipPreview = null" class="p-1 text-slate-400 hover:text-red-500">
              <X class="w-4 h-4" />
            </button>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2">
          <button
            type="button"
            @click="isApproveModalOpen = false; approvingPayout = null"
            class="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 hover:bg-slate-50 text-xs font-semibold cursor-pointer"
          >
            ยกเลิก
          </button>
          <button
            type="button"
            id="btn-confirm-approve-payout"
            @click="confirmApprove"
            :disabled="isProcessing"
            class="px-5 py-2.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold transition-colors disabled:opacity-50 cursor-pointer flex items-center gap-1.5"
          >
            <span v-if="isProcessing" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span>ยืนยันการโอนเงินสำเร็จ</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Reject Modal -->
    <div
      v-if="isRejectModalOpen && rejectingPayout"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs"
    >
      <div class="bg-white rounded-3xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
        <h3 class="text-base font-bold text-slate-800 mb-2">ระบุเหตุผลการปฏิเสธคำขอถอนเงิน</h3>
        <p class="text-xs text-slate-500 mb-4">
          ยอดเงิน <strong>{{ formatCurrency(rejectingPayout.amount) }}</strong> จะถูกคืนกลับเข้ากระเป๋าของพี่เลี้ยงโดยอัตโนมัติ
        </p>

        <textarea
          v-model="rejectReason"
          rows="3"
          placeholder="เช่น เลขบัญชีไม่ถูกต้อง หรือชื่อบัญชีไม่ตรงกับชื่อผู้ใช้..."
          class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-hidden focus:border-rose-500 focus:ring-2 focus:ring-rose-200 mb-4"
        ></textarea>

        <div class="flex items-center justify-end gap-2">
          <button
            type="button"
            @click="isRejectModalOpen = false; rejectingPayout = null"
            class="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 hover:bg-slate-50 text-xs font-semibold cursor-pointer"
          >
            ยกเลิก
          </button>
          <button
            type="button"
            id="btn-confirm-reject-payout"
            @click="confirmReject"
            :disabled="isProcessing"
            class="px-4 py-2 rounded-xl bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold transition-colors disabled:opacity-50 cursor-pointer"
          >
            ยืนยันปฏิเสธและคืนเงิน
          </button>
        </div>
      </div>
    </div>

    <!-- Slip Preview Modal -->
    <div
      v-if="previewSlipUrl"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-xs"
      @click="previewSlipUrl = null"
    >
      <div class="relative max-w-lg w-full flex flex-col items-center">
        <button
          @click="previewSlipUrl = null"
          class="absolute -top-12 right-0 text-white/80 hover:text-white p-2"
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
  </div>
</template>
