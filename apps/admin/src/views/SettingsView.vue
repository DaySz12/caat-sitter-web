<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api } from '../api/client';
import { Percent, Save, CheckCircle2, AlertCircle, Database } from 'lucide-vue-next';

const serviceFeePercentage = ref(10.0);
const isSaving = ref(false);
const saveSuccess = ref(false);
const saveError = ref('');

async function loadSettings() {
  try {
    const data = await api.get<{ percentage: number }>('/api/admin/settings/service-fee');
    serviceFeePercentage.value = Number(data.percentage) || 10.0;
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

onMounted(() => {
  loadSettings();
});
</script>

<template>
  <div class="max-w-3xl space-y-6">
    <!-- Header -->
    <div>
      <h1 class="text-2xl font-bold text-slate-800">ตั้งค่าระบบ (Platform Settings)</h1>
      <p class="text-xs text-slate-500 mt-1">กำหนดอัตราค่าธรรมเนียมแพลตฟอร์มและการตั้งค่าระบบหลังบ้าน</p>
    </div>

    <!-- Alert -->
    <div
      v-if="saveSuccess"
      class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center gap-3"
    >
      <CheckCircle2 class="w-5 h-5 text-emerald-600" />
      <span>บันทึกการตั้งค่าเรียบร้อยแล้ว มีผลต่อรายการจองใหม่ทันที</span>
    </div>

    <div
      v-if="saveError"
      class="p-4 rounded-xl bg-red-50 border border-red-200 text-sm text-red-700 flex items-center gap-3"
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
              class="w-full pl-4 pr-10 py-3 rounded-xl border border-slate-200 text-slate-800 font-bold text-lg focus:outline-none focus:ring-2 focus:ring-[#FF7A59]"
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
          class="px-6 py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm shadow-md shadow-[#FF7A59]/30 transition-all flex items-center gap-2 cursor-pointer disabled:opacity-50"
        >
          <Save class="w-4 h-4" />
          <span>{{ isSaving ? 'กำลังบันทึก...' : 'บันทึกการตั้งค่า' }}</span>
        </button>
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
