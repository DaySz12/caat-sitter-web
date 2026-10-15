<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';
import {
  ShieldCheck,
  Lock,
  CheckCircle2,
  AlertCircle,
  Clock,
  FileText,
  UploadCloud,
  X,
  Eye,
  Info,
  Sparkles,
  Loader2,
  ArrowRight,
  ShieldAlert,
  Camera
} from 'lucide-vue-next';
import { sittersApi, type SitterVerificationStatus } from '@/features/sitters/sitters.api';
import { api, resolveImageUrl } from '@/shared/lib/api';

const props = defineProps<{
  isOpen: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'updated', status: SitterVerificationStatus): void;
}>();

const verification = ref<SitterVerificationStatus | null>(null);
const isLoading = ref(true);
const isUploading = ref(false);
const isSubmitting = ref(false);
const previewUrl = ref<string | null>(null);
const uploadedDocumentUrl = ref<string | null>(null);
const errorMessage = ref('');
const successMessage = ref('');

// Agreement checklist checkboxes to ensure sitter feels in full control
const agreeWatermark = ref(true);
const agreePdpa = ref(true);

const hasSubmitted = computed(() => verification.value?.status === 'pending');
const isApproved = computed(() => verification.value?.isVerified || verification.value?.status === 'approved');
const isRejected = computed(() => verification.value?.status === 'rejected');

async function loadVerificationStatus() {
  isLoading.value = true;
  errorMessage.value = '';
  try {
    const data = await sittersApi.getVerificationStatus();
    verification.value = data;
    uploadedDocumentUrl.value = data.idCardUrl;
  } catch (err: any) {
    console.error('Failed to load verification status', err);
  } finally {
    isLoading.value = false;
  }
}

watch(
  () => props.isOpen,
  (open) => {
    if (open) {
      errorMessage.value = '';
      successMessage.value = '';
      previewUrl.value = null;
      loadVerificationStatus();
    }
  }
);

async function onFileSelected(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;

  // Validation
  if (file.size > 10 * 1024 * 1024) {
    errorMessage.value = 'ขนาดไฟล์ต้องไม่เกิน 10 MB';
    return;
  }

  isUploading.value = true;
  errorMessage.value = '';

  try {
    // Show local preview immediately
    previewUrl.value = URL.createObjectURL(file);

    // Upload to secure documents bucket
    const res = await api.upload('sitter-documents', file);
    uploadedDocumentUrl.value = res.url;
  } catch (err: any) {
    errorMessage.value = err.message || 'อัปโหลดเอกสารไม่สำเร็จ';
    previewUrl.value = null;
  } finally {
    isUploading.value = false;
  }
}

async function handleSubmit() {
  if (!uploadedDocumentUrl.value) {
    errorMessage.value = 'กรุณาแนบรูปภาพเอกสารเพื่อยืนยันตัวตน';
    return;
  }

  isSubmitting.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const res = await sittersApi.submitVerification(uploadedDocumentUrl.value);
    verification.value = res;
    successMessage.value = 'ยื่นเอกสารเรียบร้อยแล้ว! เจ้าหน้าที่จะตรวจสอบข้อมูลและอนุมัติภายใน 24 ชม.';
    emit('updated', res);
  } catch (err: any) {
    errorMessage.value = err.message || 'ไม่สามารถส่งข้อมูลยืนยันตัวตนได้';
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 overflow-y-auto bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 sm:p-6"
    @click.self="emit('close')"
  >
    <div class="bg-white rounded-3xl max-w-xl w-full max-h-[92vh] flex flex-col shadow-2xl border border-slate-100 overflow-hidden animate-in fade-in zoom-in-95 duration-150">
      <!-- Modal Header -->
      <div class="px-6 py-5 border-b border-slate-100 flex items-center justify-between bg-slate-50/70 shrink-0">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center font-bold shadow-xs">
            <ShieldCheck class="w-5 h-5" />
          </div>
          <div>
            <h3 class="font-bold text-slate-800 text-base">ยืนยันตัวตนพี่เลี้ยง (Identity Verification)</h3>
            <p class="text-xs text-slate-500">รับตราสัญลักษณ์ Verified เพื่อเพิ่มความน่าเชื่อถือและงานดูแล</p>
          </div>
        </div>
        <button
          class="p-1.5 rounded-xl text-slate-400 hover:text-slate-600 hover:bg-slate-100 transition"
          @click="emit('close')"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Modal Body -->
      <div class="p-6 space-y-5 overflow-y-auto flex-1 text-sm text-slate-700">
        <!-- Loading State -->
        <div v-if="isLoading" class="py-12 text-center text-slate-400">
          <Loader2 class="w-8 h-8 animate-spin mx-auto mb-2 text-[#FF7A59]" />
          <p class="text-xs">กำลังโหลดข้อมูลการยืนยันตัวตน...</p>
        </div>

        <template v-else>
          <!-- Status Banner: Approved -->
          <div
            v-if="isApproved"
            class="p-5 rounded-2xl bg-emerald-50 border border-emerald-200/80 text-emerald-800 flex items-start gap-3.5"
          >
            <div class="w-8 h-8 rounded-full bg-emerald-500 text-white flex items-center justify-center shrink-0 mt-0.5">
              <CheckCircle2 class="w-5 h-5" />
            </div>
            <div>
              <h4 class="font-bold text-emerald-900 text-sm">คุณผ่านการยืนยันตัวตนเรียบร้อยแล้ว (Verified Sitter)</h4>
              <p class="text-xs text-emerald-700 mt-1 leading-relaxed">
                โปรไฟล์ของคุณมีตราสัญลักษณ์รับรองความปลอดภัย ลูกค้าสามารถไว้วางใจและระบบจะจัดอันดับแสดงในผลการค้นหาอันดับต้นๆ
              </p>
            </div>
          </div>

          <!-- Status Banner: Pending Review -->
          <div
            v-else-if="hasSubmitted"
            class="p-5 rounded-2xl bg-amber-50 border border-amber-200/80 text-amber-900 flex items-start gap-3.5"
          >
            <div class="w-8 h-8 rounded-full bg-amber-500 text-white flex items-center justify-center shrink-0 mt-0.5">
              <Clock class="w-5 h-5" />
            </div>
            <div class="flex-1">
              <h4 class="font-bold text-amber-900 text-sm">เอกสารของคุณอยู่ระหว่างการตรวจสอบ</h4>
              <p class="text-xs text-amber-700 mt-1 leading-relaxed">
                เราได้รับเอกสารยืนยันตัวตนของคุณแล้ว ทีมงานจะตรวจสอบความถูกต้องและแจ้งผลให้ทราบภายใน 24 ชั่วโมง
              </p>
              <div v-if="verification?.idCardUrl" class="mt-3 inline-flex items-center gap-2 px-3 py-1.5 rounded-xl bg-white border border-amber-200 text-xs font-semibold text-amber-800">
                <FileText class="w-3.5 h-3.5 text-amber-600" />
                <span>แนบเอกสารเรียบร้อยแล้ว (จัดเก็บอย่างปลอดภัย)</span>
              </div>
            </div>
          </div>

          <!-- Status Banner: Rejected -->
          <div
            v-else-if="isRejected"
            class="p-5 rounded-2xl bg-rose-50 border border-rose-200/80 text-rose-900 flex items-start gap-3.5"
          >
            <div class="w-8 h-8 rounded-full bg-rose-500 text-white flex items-center justify-center shrink-0 mt-0.5">
              <AlertCircle class="w-5 h-5" />
            </div>
            <div class="flex-1">
              <h4 class="font-bold text-rose-900 text-sm">เอกสารยังไม่ผ่านการอนุมัติ</h4>
              <p class="text-xs text-rose-700 mt-1">
                <strong>เหตุผล:</strong> {{ verification?.rejectionReason || 'เอกสารไม่ชัดเจนหรือไม่ตรงกับชื่อบัญชี' }}
              </p>
              <p class="text-xs text-rose-600 mt-1">
                คุณสามารถถ่ายภาพและอัปโหลดเอกสารใหม่ด้านล่างนี้เพื่อส่งตรวจอีกครั้งได้เลยครับ
              </p>
            </div>
          </div>

          <!-- Benefits of being Verified -->
          <div v-if="!isApproved" class="p-4 rounded-2xl bg-gradient-to-r from-orange-50 to-amber-50/60 border border-orange-100">
            <h4 class="text-xs font-bold text-slate-800 flex items-center gap-1.5 mb-2">
              <Sparkles class="w-4 h-4 text-[#FF7A59]" /> สิทธิประโยชน์ของพี่เลี้ยงที่ยืนยันตัวตนแล้ว
            </h4>
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-2.5 text-xs text-slate-600">
              <div class="flex items-center gap-2 p-2 rounded-xl bg-white/80 border border-orange-100/60">
                <span class="text-emerald-500 font-bold">✓</span>
                <span>ลูกค้าจองเร็วกว่า 3 เท่า</span>
              </div>
              <div class="flex items-center gap-2 p-2 rounded-xl bg-white/80 border border-orange-100/60">
                <span class="text-emerald-500 font-bold">✓</span>
                <span>ดันโปรไฟล์ขึ้นหน้าแรก</span>
              </div>
              <div class="flex items-center gap-2 p-2 rounded-xl bg-white/80 border border-orange-100/60">
                <span class="text-emerald-500 font-bold">✓</span>
                <span>ป้ายการันตีความปลอดภัย</span>
              </div>
            </div>
          </div>

          <!-- Trust & Privacy Safety Guarantee Box (Crucial for peace of mind!) -->
          <div v-if="!isApproved" class="p-4.5 rounded-2xl bg-slate-50 border border-slate-200/80 space-y-3">
            <div class="flex items-center gap-2 text-slate-800 font-bold text-xs">
              <Lock class="w-4 h-4 text-[#FF7A59]" />
              <span>ความปลอดภัยและความเป็นส่วนตัวของข้อมูล (PDPA 100%)</span>
            </div>

            <p class="text-[11px] text-slate-500 leading-relaxed">
              เราเข้าใจดีถึงความกังวลเรื่องข้อมูลส่วนตัว เพื่อให้คุณสบายใจและมั่นใจได้อย่างเต็มที่:
            </p>

            <ul class="space-y-2 text-xs text-slate-600">
              <li class="flex items-start gap-2">
                <span class="w-4 h-4 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-[10px] font-bold shrink-0 mt-0.5">1</span>
                <div>
                  <strong>อนุญาตให้ขีดฆ่าทับเอกสารได้:</strong>
                  <span class="text-slate-500"> สามารถใช้ปากกาหรือแอปเขียนทับว่า <em>"ใช้สำหรับยืนยันตัวตน Cat Sitter เท่านั้น"</em></span>
                </div>
              </li>
              <li class="flex items-start gap-2">
                <span class="w-4 h-4 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-[10px] font-bold shrink-0 mt-0.5">2</span>
                <div>
                  <strong>ปิดบัง Laser ID และข้อมูลลับได้:</strong>
                  <span class="text-slate-500"> คุณสามารถปิดหรือเบลอเลขรหัสหลังบัตร (Laser ID) ได้ทันที เราต้องการตรวจสอบเฉพาะ <em>ชื่อ-นามสกุล และรูปถ่าย</em> ให้ตรงกับโปรไฟล์เท่านั้น</span>
                </div>
              </li>
              <li class="flex items-start gap-2">
                <span class="w-4 h-4 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-[10px] font-bold shrink-0 mt-0.5">3</span>
                <div>
                  <strong>เอกสารนี้ไม่เปิดเผยต่อสาธารณะ:</strong>
                  <span class="text-slate-500"> มีเพียงผู้ดูแลระบบ (Admin) เท่านั้นที่ตรวจสอบเพื่ออนุมัติ เจ้าของแมวและคนภายนอกจะไม่เห็นรูปนี้เด็ดขาด</span>
                </div>
              </li>
            </ul>
          </div>

          <!-- Document Upload Section -->
          <div v-if="!isApproved" class="space-y-3">
            <label class="block text-xs font-bold text-slate-800">
              แนบเอกสารยืนยันตัวตน (บัตรประชาชน / ใบขับขี่ / หน้าสมุดบัญชีธนาคาร)
            </label>

            <!-- Upload Area / Preview -->
            <div
              v-if="previewUrl || uploadedDocumentUrl"
              class="relative rounded-2xl overflow-hidden border border-slate-200 bg-slate-100 max-h-56 flex items-center justify-center group"
            >
              <img
                :src="previewUrl || resolveImageUrl(uploadedDocumentUrl)"
                class="max-h-56 w-full object-contain p-2"
                alt="เอกสารยืนยันตัวตน"
              />
              <label
                class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition flex items-center justify-center text-white text-xs font-semibold gap-2 cursor-pointer backdrop-blur-xs"
              >
                <Camera class="w-4 h-4" /> เปลี่ยนรูปเอกสาร
                <input type="file" accept="image/*" class="hidden" @change="onFileSelected" />
              </label>
            </div>

            <!-- Upload Dropzone -->
            <label
              v-else
              class="border-2 border-dashed border-slate-200 hover:border-[#FF7A59] rounded-2xl p-6 text-center cursor-pointer transition flex flex-col items-center justify-center bg-slate-50 hover:bg-[#FFF0EB]/20 group"
            >
              <Loader2 v-if="isUploading" class="w-8 h-8 text-[#FF7A59] animate-spin mb-2" />
              <template v-else>
                <div class="w-12 h-12 rounded-2xl bg-white border border-slate-200 text-slate-400 group-hover:text-[#FF7A59] group-hover:border-[#FF7A59] flex items-center justify-center shadow-xs transition mb-2">
                  <UploadCloud class="w-6 h-6" />
                </div>
                <p class="text-xs font-bold text-slate-700 group-hover:text-[#FF7A59]">
                  คลิกเพื่อเลือกไฟล์รูปถ่ายเอกสาร
                </p>
                <p class="text-[11px] text-slate-400 mt-1">
                  รองรับไฟล์ภาพ JPG, PNG (ขนาดไม่เกิน 10MB)
                </p>
              </template>
              <input type="file" accept="image/*" class="hidden" :disabled="isUploading" @change="onFileSelected" />
            </label>

            <!-- Alerts -->
            <div v-if="errorMessage" class="p-3 rounded-xl bg-red-50 border border-red-200 text-xs text-red-700 flex items-center gap-2">
              <AlertCircle class="w-4 h-4 shrink-0" />
              <span>{{ errorMessage }}</span>
            </div>
            <div v-if="successMessage" class="p-3 rounded-xl bg-emerald-50 border border-emerald-200 text-xs text-emerald-800 flex items-center gap-2">
              <CheckCircle2 class="w-4 h-4 shrink-0" />
              <span>{{ successMessage }}</span>
            </div>
          </div>
        </template>
      </div>

      <!-- Modal Footer -->
      <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex items-center justify-between gap-3 shrink-0">
        <button
          type="button"
          class="px-4 py-2.5 rounded-xl border border-slate-200 text-slate-600 font-semibold text-xs hover:bg-slate-100 transition"
          @click="emit('close')"
        >
          {{ isApproved ? 'ปิดหน้าต่าง' : 'ไว้ทำภายหลัง' }}
        </button>

        <button
          v-if="!isApproved"
          type="button"
          :disabled="isSubmitting || isUploading || !uploadedDocumentUrl"
          class="px-6 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-bold text-xs flex items-center gap-2 shadow-sm transition disabled:opacity-40"
          @click="handleSubmit"
        >
          <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
          <ShieldCheck v-else class="w-4 h-4" />
          <span>{{ hasSubmitted ? 'อัปเดตเอกสารใหม่' : 'ยื่นขอรับการยืนยันตัวตน' }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
