<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import {
  Banknote,
  X,
  CheckCircle2,
  AlertCircle,
  Clock,
  Building2,
  History,
  ArrowRight,
  Eye
} from 'lucide-vue-next';
import { payoutsApi, type CreatePayoutPayload } from './payouts.api';
import type { PayoutDto } from '@/shared/types/models';
import { formatCurrency, formatRelativeTime } from '@/shared/lib/format';
import { API_URL } from '@/shared/lib/api';

const props = defineProps<{
  isOpen: boolean;
  walletBalance: number;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submitted', updatedPayout: PayoutDto): void;
}>();

const currentTab = ref<'request' | 'history'>('request');
const payouts = ref<PayoutDto[]>([]);
const isLoadingHistory = ref(false);

const amount = ref<number | ''>('');
const selectedBank = ref('กสิกรไทย (KBANK)');
const customBank = ref('');
const accountNumber = ref('');
const accountName = ref('');
const promptPayNumber = ref('');

const isSubmitting = ref(false);
const errorMsg = ref('');
const successMsg = ref('');

const previewSlipUrl = ref<string | null>(null);

const commonBanks = [
  'กสิกรไทย (KBANK)',
  'ไทยพาณิชย์ (SCB)',
  'กรุงเทพ (BBL)',
  'กรุงไทย (KTB)',
  'ทหารไทยธนชาต (TTB)',
  'กรุงศรีอยุธยา (BAY)',
  'ออมสิน (GSB)',
  'พร้อมเพย์ (PromptPay)',
  'อื่นๆ (ระบุ)'
];

const effectiveBankName = computed(() => {
  return selectedBank.value === 'อื่นๆ (ระบุ)' ? customBank.value.trim() : selectedBank.value;
});

async function loadHistory() {
  isLoadingHistory.value = true;
  try {
    payouts.value = await payoutsApi.mine();
  } catch (e: any) {
    console.error('Error loading payout history:', e);
  } finally {
    isLoadingHistory.value = false;
  }
}

function setMaxAmount() {
  amount.value = Math.max(0, props.walletBalance);
}

async function handleSubmit() {
  errorMsg.value = '';
  successMsg.value = '';

  const numAmount = Number(amount.value);
  if (isNaN(numAmount) || numAmount < 50) {
    errorMsg.value = 'ยอดเงินขั้นต่ำในการถอนคือ 50 บาท';
    return;
  }
  if (numAmount > props.walletBalance) {
    errorMsg.value = `ยอดเงินในกระเป๋าไม่เพียงพอ (คงเหลือ ฿${props.walletBalance.toLocaleString('th-TH')})`;
    return;
  }
  if (!effectiveBankName.value) {
    errorMsg.value = 'กรุณาระบุชื่อธนาคาร';
    return;
  }
  if (!accountNumber.value.trim()) {
    errorMsg.value = 'กรุณากรอกเลขที่บัญชี';
    return;
  }
  if (!accountName.value.trim()) {
    errorMsg.value = 'กรุณากรอกชื่อบัญชี';
    return;
  }

  isSubmitting.value = true;
  try {
    const payload: CreatePayoutPayload = {
      amount: numAmount,
      bankName: effectiveBankName.value,
      bankAccountNumber: accountNumber.value.trim(),
      bankAccountName: accountName.value.trim(),
      promptPayNumber: promptPayNumber.value.trim() || undefined
    };

    const result = await payoutsApi.request(payload);
    successMsg.value = `ยื่นคำขอถอนเงิน ${formatCurrency(numAmount)} สำเร็จ เจ้าหน้าที่จะโอนเงินให้โดยเร็ว`;
    emit('submitted', result);
    amount.value = '';
    loadHistory();
    setTimeout(() => {
      currentTab.value = 'history';
      successMsg.value = '';
    }, 2000);
  } catch (e: any) {
    errorMsg.value = e.message || 'ส่งคำขอถอนเงินไม่สำเร็จ กรุณาลองใหม่อีกครั้ง';
  } finally {
    isSubmitting.value = false;
  }
}

function resolveImageUrl(url?: string | null) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
}

watch(
  () => props.isOpen,
  (open) => {
    if (open) {
      errorMsg.value = '';
      successMsg.value = '';
      loadHistory();
    }
  }
);

onMounted(() => {
  if (props.isOpen) loadHistory();
});
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 overflow-y-auto"
  >
    <div class="bg-white rounded-3xl max-w-lg w-full shadow-2xl border border-slate-100 overflow-hidden my-6">
      <!-- Header -->
      <div class="px-6 py-4 bg-slate-900 text-white flex items-center justify-between">
        <div>
          <h3 class="font-bold text-base flex items-center gap-2">
            <Banknote class="w-5 h-5 text-emerald-400" />
            กระเป๋าเงิน & ถอนเงินเข้าบัญชี
          </h3>
          <p class="text-xs text-slate-300 mt-0.5">
            ยอดเงินที่สามารถถอนได้: <strong class="text-emerald-400 font-mono text-sm">{{ formatCurrency(walletBalance) }}</strong>
          </p>
        </div>
        <button
          type="button"
          @click="emit('close')"
          class="text-slate-400 hover:text-white p-1 rounded-lg hover:bg-slate-800 transition cursor-pointer"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Tabs -->
      <div class="flex border-b border-slate-100 bg-slate-50 px-6 pt-3 gap-4">
        <button
          type="button"
          @click="currentTab = 'request'"
          :class="[
            'pb-3 text-xs font-bold border-b-2 transition cursor-pointer flex items-center gap-1.5',
            currentTab === 'request'
              ? 'border-emerald-600 text-emerald-700'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          ]"
        >
          <Banknote class="w-4 h-4" />
          แบบฟอร์มขอถอนเงิน
        </button>
        <button
          type="button"
          @click="currentTab = 'history'"
          :class="[
            'pb-3 text-xs font-bold border-b-2 transition cursor-pointer flex items-center gap-1.5',
            currentTab === 'history'
              ? 'border-emerald-600 text-emerald-700'
              : 'border-transparent text-slate-500 hover:text-slate-800'
          ]"
        >
          <History class="w-4 h-4" />
          ประวัติการถอนเงิน ({{ payouts.length }})
        </button>
      </div>

      <!-- Tab Content: Request Form -->
      <div v-if="currentTab === 'request'" class="p-6 space-y-4 max-h-[75vh] overflow-y-auto">
        <!-- Balance Highlight -->
        <div class="bg-gradient-to-br from-emerald-50 to-teal-50 border border-emerald-200/60 rounded-2xl p-4 flex items-center justify-between">
          <div>
            <span class="text-xs text-slate-500 font-medium">ยอดเงินคงเหลือในกระเป๋า</span>
            <p class="text-2xl font-black text-emerald-700 mt-0.5">{{ formatCurrency(walletBalance) }}</p>
          </div>
          <button
            type="button"
            @click="setMaxAmount"
            class="px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold rounded-xl shadow-xs cursor-pointer transition"
          >
            ถอนทั้งหมด
          </button>
        </div>

        <div v-if="successMsg" class="p-3.5 bg-emerald-50 border border-emerald-200 text-emerald-800 rounded-xl text-xs flex items-center gap-2">
          <CheckCircle2 class="w-4 h-4 text-emerald-600 shrink-0" />
          <span>{{ successMsg }}</span>
        </div>

        <div v-if="errorMsg" class="p-3.5 bg-rose-50 border border-rose-200 text-rose-800 rounded-xl text-xs flex items-center gap-2">
          <AlertCircle class="w-4 h-4 text-rose-600 shrink-0" />
          <span>{{ errorMsg }}</span>
        </div>

        <form @submit.prevent="handleSubmit" class="space-y-4">
          <!-- Amount Input -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-1.5">
              จำนวนเงินที่ต้องการถอน (บาท) *
            </label>
            <div class="relative">
              <span class="absolute left-3.5 top-1/2 -translate-y-1/2 font-bold text-slate-400">฿</span>
              <input
                v-model.number="amount"
                type="number"
                min="50"
                :max="walletBalance"
                step="1"
                placeholder="ยอดเงินขั้นต่ำ 50 บาท"
                required
                class="w-full pl-8 pr-4 py-2.5 rounded-xl border border-slate-200 text-slate-800 font-bold text-base focus:outline-hidden focus:border-emerald-600 focus:ring-2 focus:ring-emerald-500/20"
              />
            </div>
          </div>

          <!-- Bank Selection -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-1.5">
              ธนาคารผู้รับเงิน *
            </label>
            <select
              v-model="selectedBank"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 text-sm focus:outline-hidden focus:border-emerald-600 bg-white"
            >
              <option v-for="b in commonBanks" :key="b" :value="b">{{ b }}</option>
            </select>
          </div>

          <div v-if="selectedBank === 'อื่นๆ (ระบุ)'">
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-1.5">
              ระบุชื่อธนาคาร *
            </label>
            <input
              v-model="customBank"
              type="text"
              placeholder="เช่น ธนาคารแลนด์ แอนด์ เฮ้าส์"
              required
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 text-sm focus:outline-hidden focus:border-emerald-600"
            />
          </div>

          <!-- Account Number -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-1.5">
              เลขที่บัญชีธนาคาร *
            </label>
            <input
              v-model="accountNumber"
              type="text"
              placeholder="เช่น 123-4-56789-0"
              required
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 font-mono text-sm focus:outline-hidden focus:border-emerald-600"
            />
          </div>

          <!-- Account Name -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-1.5">
              ชื่อเจ้าของบัญชี (ตรงกับสมุดบัญชี) *
            </label>
            <input
              v-model="accountName"
              type="text"
              placeholder="เช่น สมชาย ใจดี"
              required
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 text-sm focus:outline-hidden focus:border-emerald-600"
            />
          </div>

          <!-- PromptPay Optional -->
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-slate-700 mb-1.5">
              หมายเลขพร้อมเพย์ (ไม่บังคับ)
            </label>
            <input
              v-model="promptPayNumber"
              type="text"
              placeholder="เบอร์โทรศัพท์ หรือเลขบัตรประชาชน"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 font-mono text-sm focus:outline-hidden focus:border-emerald-600"
            />
          </div>

          <div class="pt-2">
            <button
              type="submit"
              :disabled="isSubmitting || walletBalance < 50"
              class="w-full py-3 px-4 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-bold text-sm shadow-md shadow-emerald-600/20 disabled:opacity-50 transition cursor-pointer flex items-center justify-center gap-2"
            >
              <span v-if="isSubmitting" class="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <span>{{ isSubmitting ? 'กำลังส่งคำขอ...' : 'ยืนยันและส่งคำขอถอนเงิน' }}</span>
            </button>
            <p class="text-[11px] text-slate-400 text-center mt-2">
              เจ้าหน้าที่จะตรวจสอบและโอนเงินเข้าบัญชีภายใน 24 ชม.
            </p>
          </div>
        </form>
      </div>

      <!-- Tab Content: History -->
      <div v-else class="p-6 space-y-3 max-h-[75vh] overflow-y-auto">
        <div v-if="isLoadingHistory" class="text-center py-10 text-slate-400 text-xs">
          กำลังโหลดประวัติการถอนเงิน...
        </div>
        <div v-else-if="payouts.length === 0" class="text-center py-10 text-slate-400 text-xs">
          ยังไม่มีประวัติการถอนเงิน
        </div>
        <div v-else class="space-y-3">
          <div
            v-for="p in payouts"
            :key="p.id"
            class="p-4 rounded-2xl border border-slate-200/80 bg-slate-50/50 space-y-2 text-xs"
          >
            <div class="flex items-center justify-between">
              <span class="font-black text-emerald-700 text-base">{{ formatCurrency(p.amount) }}</span>
              <!-- Status Badge -->
              <span
                v-if="p.status === 'pending'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200 flex items-center gap-1"
              >
                <Clock class="w-3 h-3 text-amber-500" /> รอเจ้าหน้าที่โอนเงิน
              </span>
              <span
                v-else-if="p.status === 'completed'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 flex items-center gap-1"
              >
                <CheckCircle2 class="w-3 h-3 text-emerald-500" /> โอนเงินสำเร็จ
              </span>
              <span
                v-else-if="p.status === 'rejected'"
                class="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200 flex items-center gap-1"
              >
                <AlertCircle class="w-3 h-3 text-rose-500" /> ถูกปฏิเสธ (คืนเงินแล้ว)
              </span>
            </div>

            <div class="text-slate-600 space-y-0.5">
              <p><strong class="text-slate-700">ธนาคาร:</strong> {{ p.bankName }} ({{ p.bankAccountNumber }})</p>
              <p><strong class="text-slate-700">ชื่อบัญชี:</strong> {{ p.bankAccountName }}</p>
              <p v-if="p.promptPayNumber"><strong class="text-slate-700">พร้อมเพย์:</strong> {{ p.promptPayNumber }}</p>
              <p class="text-slate-400 text-[11px] pt-1">
                ยื่นเมื่อ: {{ formatRelativeTime(p.createdAt) }}
                <span v-if="p.processedAt"> · ดำเนินการเมื่อ: {{ formatRelativeTime(p.processedAt) }}</span>
              </p>
            </div>

            <div v-if="p.adminNotes" class="bg-rose-50 text-rose-700 p-2 rounded-lg text-[11px] border border-rose-100">
              <strong>เหตุผล:</strong> {{ p.adminNotes }}
            </div>

            <div v-if="p.transferSlipUrl" class="pt-1">
              <button
                type="button"
                @click="previewSlipUrl = resolveImageUrl(p.transferSlipUrl)"
                class="text-emerald-700 hover:text-emerald-900 underline font-semibold text-xs inline-flex items-center gap-1 cursor-pointer"
              >
                <Eye class="w-3.5 h-3.5" /> ดูสลิปหลักฐานการโอน
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Preview Slip Modal -->
    <div
      v-if="previewSlipUrl"
      class="fixed inset-0 z-60 bg-black/80 backdrop-blur-xs flex items-center justify-center p-4"
      @click="previewSlipUrl = null"
    >
      <div class="relative max-w-sm w-full flex flex-col items-center">
        <button
          @click="previewSlipUrl = null"
          class="absolute -top-10 right-0 text-white p-2"
        >
          <X class="w-6 h-6" />
        </button>
        <img
          :src="previewSlipUrl"
          alt="Slip"
          class="max-w-full max-h-[80vh] rounded-2xl object-contain bg-white shadow-2xl"
          @click.stop
        />
      </div>
    </div>
  </div>
</template>
