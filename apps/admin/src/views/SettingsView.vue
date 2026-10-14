<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api } from '../api/client';
import { Percent, Save, CheckCircle2, AlertCircle, Database, CreditCard } from 'lucide-vue-next';

const serviceFeePercentage = ref(10.0);
const isSaving = ref(false);
const saveSuccess = ref(false);
const saveError = ref('');

// Payment settings state
const paymentSetting = ref({
  bankName: 'กสิกรไทย (KBANK)',
  bankAccountNumber: '123-2-88990-0',
  bankAccountName: 'บจก. แคท ซิตเตอร์ (ไทยแลนด์)',
  promptPayNumber: '010-5-56600-0000',
  promptPayName: 'บจก. แคท ซิตเตอร์ (ไทยแลนด์)'
});
const isSavingPayment = ref(false);
const paymentSaveSuccess = ref(false);
const paymentSaveError = ref('');

async function loadSettings() {
  try {
    const [feeData, paymentData] = await Promise.all([
      api.get<{ percentage: number }>('/api/admin/settings/service-fee').catch(() => null),
      api.get<{
        bankName: string;
        bankAccountNumber: string;
        bankAccountName: string;
        promptPayNumber: string;
        promptPayName: string;
      }>('/api/admin/settings/payment-info').catch(() => null)
    ]);

    if (feeData) serviceFeePercentage.value = Number(feeData.percentage) || 10.0;
    if (paymentData) {
      paymentSetting.value = {
        bankName: paymentData.bankName || 'กสิกรไทย (KBANK)',
        bankAccountNumber: paymentData.bankAccountNumber || '123-2-88990-0',
        bankAccountName: paymentData.bankAccountName || 'บจก. แคท ซิตเตอร์ (ไทยแลนด์)',
        promptPayNumber: paymentData.promptPayNumber || '010-5-56600-0000',
        promptPayName: paymentData.promptPayName || 'บจก. แคท ซิตเตอร์ (ไทยแลนด์)'
      };
    }
  } catch (e) {
    console.error('Error loading settings:', e);
  }
}

async function handleSaveFee() {
  isSaving.value = true;
  saveSuccess.value = false;
  saveError.value = '';

  try {
    await api.put('/api/admin/settings/service-fee', { percentage: serviceFeePercentage.value });
    saveSuccess.value = true;
    setTimeout(() => {
      saveSuccess.value = false;
    }, 3000);
  } catch (e: any) {
    saveError.value = e.message || 'บันทึกไม่สำเร็จ';
  } finally {
    isSaving.value = false;
  }
}

async function handleSavePayment() {
  isSavingPayment.value = true;
  paymentSaveSuccess.value = false;
  paymentSaveError.value = '';

  try {
    await api.put('/api/admin/settings/payment-info', paymentSetting.value);
    paymentSaveSuccess.value = true;
    setTimeout(() => {
      paymentSaveSuccess.value = false;
    }, 3000);
  } catch (e: any) {
    paymentSaveError.value = e.message || 'บันทึกไม่สำเร็จ';
  } finally {
    isSavingPayment.value = false;
  }
}

onMounted(loadSettings);
</script>

<template>
  <div class="max-w-4xl space-y-6">
    <!-- Header -->
    <div>
      <h1 class="text-2xl font-bold text-slate-800">ตั้งค่าระบบ (Platform Settings)</h1>
      <p class="text-xs text-slate-500 mt-1">กำหนดอัตราค่าธรรมเนียมแพลตฟอร์ม บัญชีรับเงิน และการตั้งค่าระบบหลังบ้าน</p>
    </div>

    <!-- Alert for Fee -->
    <div
      v-if="saveSuccess"
      class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center gap-3 animate-fade-in"
    >
      <CheckCircle2 class="w-5 h-5 text-emerald-600" />
      <span>บันทึกค่าธรรมเนียมเรียบร้อยแล้ว มีผลต่อรายการจองใหม่ทันที</span>
    </div>

    <div
      v-if="saveError"
      class="p-4 rounded-xl bg-red-50 border border-red-200 text-sm text-red-700 flex items-center gap-3 animate-fade-in"
    >
      <AlertCircle class="w-5 h-5 text-red-600" />
      <span>{{ saveError }}</span>
    </div>

    <!-- Service Fee Settings Card -->
    <div class="bg-white rounded-2xl border border-slate-200/80 p-6 sm:p-8 shadow-xs space-y-6">
      <div class="flex items-center gap-3 pb-4 border-b border-slate-100">
        <div class="w-10 h-10 rounded-xl bg-[#FF7A59]/10 text-[#FF7A59] flex items-center justify-center">
          <Percent class="w-5 h-5" />
        </div>
        <div>
          <h3 class="font-bold text-slate-800 text-base">อัตราค่าธรรมเนียมแพลตฟอร์ม (Service Fee)</h3>
          <p class="text-xs text-slate-500">ส่วนแบ่งรายได้จากรายการจองแต่ละครั้งที่ระบบจะหักเพื่อเป็นค่าบริการ</p>
        </div>
      </div>

      <form @submit.prevent="handleSaveFee" class="space-y-5">
        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-2">
            เปอร์เซ็นต์ค่าธรรมเนียม (%)
          </label>
          <div class="relative max-w-xs">
            <input
              v-model.number="serviceFeePercentage"
              type="number"
              step="0.1"
              min="0"
              max="50"
              required
              class="w-full pl-4 pr-10 py-3 rounded-xl border border-slate-200 text-slate-800 font-bold text-lg focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
            />
            <span class="absolute right-4 top-1/2 -translate-y-1/2 font-bold text-slate-400">%</span>
          </div>
          <p class="text-xs text-slate-400 mt-2">
            ตัวอย่าง: ถ้ายอดจอง 1,000 บาท ค่าธรรมเนียม {{ serviceFeePercentage }}% จะเท่ากับ ฿{{ (1000 * serviceFeePercentage / 100).toFixed(0) }} บาท
          </p>
        </div>

        <button
          type="submit"
          :disabled="isSaving"
          class="px-6 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm shadow-md shadow-[#FF7A59]/20 transition-all flex items-center gap-2 cursor-pointer disabled:opacity-50"
        >
          <Save class="w-4 h-4" />
          <span>{{ isSaving ? 'กำลังบันทึก...' : 'บันทึกค่าธรรมเนียม' }}</span>
        </button>
      </form>
    </div>

    <!-- Alert for Payment Settings -->
    <div
      v-if="paymentSaveSuccess"
      class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center gap-3 animate-fade-in"
    >
      <CheckCircle2 class="w-5 h-5 text-emerald-600" />
      <span>บันทึกข้อมูลบัญชีธนาคารและพร้อมเพย์เรียบร้อยแล้ว มีผลต่อหน้าชำระเงินของลูกค้าทันที</span>
    </div>

    <div
      v-if="paymentSaveError"
      class="p-4 rounded-xl bg-red-50 border border-red-200 text-sm text-red-700 flex items-center gap-3 animate-fade-in"
    >
      <AlertCircle class="w-5 h-5 text-red-600" />
      <span>{{ paymentSaveError }}</span>
    </div>

    <!-- Platform Payment & PromptPay Settings Card -->
    <div class="bg-white rounded-2xl border border-slate-200/80 p-6 sm:p-8 shadow-xs space-y-6">
      <div class="flex items-center gap-3 pb-4 border-b border-slate-100">
        <div class="w-10 h-10 rounded-xl bg-blue-50 text-blue-600 flex items-center justify-center">
          <CreditCard class="w-5 h-5" />
        </div>
        <div>
          <h3 class="font-bold text-slate-800 text-base">บัญชีรับเงินและพร้อมเพย์ของแพลตฟอร์ม (Payment Accounts)</h3>
          <p class="text-xs text-slate-500">ข้อมูลนี้จะนำไปแสดงบนหน้าเว็บเพื่อให้ลูกค้าสแกน QR PromptPay หรือโอนเข้าบัญชีธนาคาร</p>
        </div>
      </div>

      <form @submit.prevent="handleSavePayment" class="space-y-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
              ชื่อธนาคาร
            </label>
            <input
              v-model="paymentSetting.bankName"
              type="text"
              required
              placeholder="เช่น กสิกรไทย (KBANK)"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 text-sm focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
              เลขที่บัญชีธนาคาร
            </label>
            <input
              v-model="paymentSetting.bankAccountNumber"
              type="text"
              required
              placeholder="เช่น 123-2-88990-0"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 font-mono text-sm focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
            ชื่อบัญชีธนาคาร
          </label>
          <input
            v-model="paymentSetting.bankAccountName"
            type="text"
            required
            placeholder="เช่น บจก. แคท ซิตเตอร์ (ไทยแลนด์)"
            class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 text-sm focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
          />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t border-slate-100">
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
              หมายเลขพร้อมเพย์ (PromptPay ID / Phone / Tax ID)
            </label>
            <input
              v-model="paymentSetting.promptPayNumber"
              type="text"
              required
              placeholder="เช่น 0105566000000 หรือ 0812345678"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 font-mono text-sm focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
            />
          </div>

          <div>
            <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-1.5">
              ชื่อบัญชีพร้อมเพย์
            </label>
            <input
              v-model="paymentSetting.promptPayName"
              type="text"
              required
              placeholder="เช่น บจก. แคท ซิตเตอร์ (ไทยแลนด์)"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-slate-800 text-sm focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
            />
          </div>
        </div>

        <div class="pt-2">
          <button
            type="submit"
            :disabled="isSavingPayment"
            class="px-6 py-2.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-semibold text-sm shadow-md shadow-blue-500/20 transition-all flex items-center gap-2 cursor-pointer disabled:opacity-50"
          >
            <Save class="w-4 h-4" />
            <span>{{ isSavingPayment ? 'กำลังบันทึก...' : 'บันทึกข้อมูลบัญชีรับเงิน' }}</span>
          </button>
        </div>
      </form>
    </div>

    <!-- Backend & Infrastructure Summary -->
    <div class="bg-white rounded-2xl border border-slate-200/80 p-6 shadow-xs space-y-4">
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl bg-slate-100 text-slate-700 flex items-center justify-center">
          <Database class="w-5 h-5" />
        </div>
        <div>
          <h3 class="font-bold text-slate-800 text-base">ข้อมูลเซิร์ฟเวอร์และฐานข้อมูล</h3>
          <p class="text-xs text-slate-500">สถานะการเชื่อมต่อบริการหลังบ้าน</p>
        </div>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
        <div class="p-3.5 bg-slate-50 rounded-xl border border-slate-200/60">
          <span class="text-slate-400 font-medium">Backend:</span>
          <p class="font-mono font-bold text-slate-800 mt-0.5">ASP.NET Core API + PostgreSQL</p>
        </div>
        <div class="p-3.5 bg-slate-50 rounded-xl border border-slate-200/60">
          <span class="text-slate-400 font-medium">Hosting:</span>
          <p class="font-bold text-slate-800 mt-0.5">Self-hosted via Docker</p>
        </div>
      </div>
    </div>
  </div>
</template>
