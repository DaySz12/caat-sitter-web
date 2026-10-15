<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { api, resolveImageUrl } from '../api/client';
import {
  Check,
  X,
  Eye,
  FileText,
  Phone,
  MapPin,
  Loader2,
  ExternalLink
} from 'lucide-vue-next';

interface SitterItem {
  id: string;
  name: string;
  email: string;
  phone: string;
  avatar_url: string;
  status: string;
  rejection_reason?: string;
  created_at: string;
  profile?: {
    bio: string;
    price_per_day: number;
    experience_years: number;
    max_cats_per_day: number;
    address: string;
    id_card_url: string;
    service_photos: string[];
    is_verified: boolean;
  };
}

const activeTab = ref<'pending' | 'approved' | 'rejected'>('pending');
const sitters = ref<SitterItem[]>([]);
const isLoading = ref(true);
const selectedSitter = ref<SitterItem | null>(null);
const isModalOpen = ref(false);
const isProcessing = ref(false);
const rejectReason = ref('');
const isRejectModalOpen = ref(false);

async function loadSitters() {
  isLoading.value = true;
  try {
    sitters.value = await api.get<SitterItem[]>(`/api/admin/sitters?status=${activeTab.value}`);
  } catch (e) {
    console.error('Error loading sitters:', e);
  } finally {
    isLoading.value = false;
  }
}

function openVerificationModal(sitter: SitterItem) {
  selectedSitter.value = sitter;
  isModalOpen.value = true;
}

async function approveSitter(sitter: SitterItem) {
  isProcessing.value = true;
  try {
    await api.post(`/api/admin/sitters/${sitter.id}/approve`);
    isModalOpen.value = false;
    await loadSitters();
  } catch (e) {
    console.error('Error approving sitter:', e);
  } finally {
    isProcessing.value = false;
  }
}

function openRejectPrompt() {
  rejectReason.value = '';
  isRejectModalOpen.value = true;
}

async function confirmReject() {
  if (!selectedSitter.value) return;
  isProcessing.value = true;
  try {
    const reason = rejectReason.value.trim() || undefined;
    await api.post(`/api/admin/sitters/${selectedSitter.value.id}/reject`, { reason });

    isRejectModalOpen.value = false;
    isModalOpen.value = false;
    await loadSitters();
  } catch (e) {
    console.error('Error rejecting sitter:', e);
  } finally {
    isProcessing.value = false;
  }
}

onMounted(() => {
  loadSitters();
});
</script>

<template>
  <div class="space-y-6">
    <!-- Header Section -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">ตรวจสอบและอนุมัติผู้รับเลี้ยง (Sitter Verification)</h1>
        <p class="text-xs text-slate-500 mt-1">ตรวจสอบเอกสารระบุตัวตน รูปสถานที่ และข้อมูลบริการก่อนเปิดใช้งาน</p>
      </div>

      <!-- Tab Switcher -->
      <div class="flex p-1 bg-slate-200/70 rounded-xl self-start sm:self-auto">
        <button
          @click="activeTab = 'pending'; loadSitters()"
          :class="[
            'px-4 py-2 text-xs font-bold rounded-lg transition-all cursor-pointer',
            activeTab === 'pending' ? 'bg-white text-[#FF7A59] shadow-xs' : 'text-slate-600 hover:text-slate-900'
          ]"
        >
          รอตรวจสอบ
        </button>
        <button
          @click="activeTab = 'approved'; loadSitters()"
          :class="[
            'px-4 py-2 text-xs font-bold rounded-lg transition-all cursor-pointer',
            activeTab === 'approved' ? 'bg-white text-emerald-600 shadow-xs' : 'text-slate-600 hover:text-slate-900'
          ]"
        >
          อนุมัติแล้ว
        </button>
        <button
          @click="activeTab = 'rejected'; loadSitters()"
          :class="[
            'px-4 py-2 text-xs font-bold rounded-lg transition-all cursor-pointer',
            activeTab === 'rejected' ? 'bg-white text-rose-600 shadow-xs' : 'text-slate-600 hover:text-slate-900'
          ]"
        >
          ถูกปฏิเสธ
        </button>
      </div>
    </div>

    <!-- Table Container -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-xs overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-slate-600">
          <thead class="bg-slate-50 text-xs uppercase font-semibold text-slate-500 border-b border-slate-100">
            <tr>
              <th class="py-3.5 px-6">ผู้รับเลี้ยง (Sitter)</th>
              <th class="py-3.5 px-6">ข้อมูลติดต่อ</th>
              <th class="py-3.5 px-6">ราคา/วัน</th>
              <th class="py-3.5 px-6">ประสบการณ์</th>
              <th class="py-3.5 px-6">วันที่สมัคร</th>
              <th class="py-3.5 px-6 text-right">การจัดการ</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="isLoading" class="text-center py-8">
              <td colspan="6" class="py-8 text-slate-400">กำลังโหลดข้อมูล...</td>
            </tr>
            <tr v-else-if="sitters.length === 0" class="text-center py-8">
              <td colspan="6" class="py-8 text-slate-400">ไม่มีรายการในหมวดหมู่นี้</td>
            </tr>
            <tr v-for="sitter in sitters" :key="sitter.id" class="hover:bg-slate-50/60 transition-colors">
              <td class="py-4 px-6">
                <div class="flex items-center gap-3">
                  <div class="w-10 h-10 rounded-full bg-slate-100 border border-slate-200 overflow-hidden shrink-0">
                    <img v-if="sitter.avatar_url" :src="resolveImageUrl(sitter.avatar_url)" class="w-full h-full object-cover" />
                    <div v-else class="w-full h-full flex items-center justify-center font-bold text-slate-500">
                      {{ sitter.name.charAt(0) }}
                    </div>
                  </div>
                  <div>
                    <p class="font-semibold text-slate-800">{{ sitter.name }}</p>
                    <p class="text-xs text-slate-400">{{ sitter.email }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-6 text-xs text-slate-600">
                <p class="flex items-center gap-1.5"><Phone class="w-3.5 h-3.5 text-slate-400" /> {{ sitter.phone || 'ไม่ระบุ' }}</p>
              </td>
              <td class="py-4 px-6 font-semibold text-slate-800">
                ฿{{ sitter.profile?.price_per_day || 300 }}
              </td>
              <td class="py-4 px-6 text-xs text-slate-600">
                {{ sitter.profile?.experience_years || 1 }} ปี (รับสูงสุด {{ sitter.profile?.max_cats_per_day || 3 }} ตัว)
              </td>
              <td class="py-4 px-6 text-xs text-slate-500">
                {{ new Date(sitter.created_at).toLocaleDateString('th-TH') }}
              </td>
              <td class="py-4 px-6 text-right">
                <button
                  @click="openVerificationModal(sitter)"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-[#FF7A59]/10 hover:bg-[#FF7A59]/20 text-[#FF7A59] font-semibold text-xs transition-colors cursor-pointer"
                >
                  <Eye class="w-4 h-4" /> ดูเอกสาร & ตรวจสอบ
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Verification Detail Modal -->
    <div
      v-if="isModalOpen && selectedSitter"
      class="fixed inset-0 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 z-50 overflow-y-auto"
    >
      <div class="bg-white rounded-3xl max-w-2xl w-full max-h-[90vh] flex flex-col shadow-2xl border border-slate-100 overflow-hidden">
        <!-- Modal Header -->
        <div class="px-6 py-5 border-b border-slate-100 flex items-center justify-between bg-slate-50/50">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 rounded-full bg-[#FF7A59]/15 text-[#FF7A59] flex items-center justify-center font-bold">
              {{ selectedSitter.name.charAt(0) }}
            </div>
            <div>
              <h3 class="font-bold text-slate-800 text-base">{{ selectedSitter.name }}</h3>
              <p class="text-xs text-slate-500">ตรวจสอบเอกสารการสมัครเป็นผู้รับเลี้ยง</p>
            </div>
          </div>
          <button @click="isModalOpen = false" class="text-slate-400 hover:text-slate-600 p-1 cursor-pointer">
            <X class="w-5 h-5" />
          </button>
        </div>

        <!-- Modal Body Content -->
        <div class="p-6 space-y-6 overflow-y-auto flex-1 text-sm text-slate-700">
          <!-- Contact & Bio Grid -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 bg-slate-50 p-4 rounded-2xl border border-slate-200/60">
            <div>
              <span class="text-xs text-slate-400 font-medium">อีเมล:</span>
              <p class="font-semibold text-slate-800">{{ selectedSitter.email }}</p>
            </div>
            <div>
              <span class="text-xs text-slate-400 font-medium">เบอร์โทรศัพท์:</span>
              <p class="font-semibold text-slate-800">{{ selectedSitter.phone || '-' }}</p>
            </div>
            <div>
              <span class="text-xs text-slate-400 font-medium">อัตราค่าบริการ:</span>
              <p class="font-semibold text-[#FF7A59]">฿{{ selectedSitter.profile?.price_per_day }} / วัน</p>
            </div>
            <div>
              <span class="text-xs text-slate-400 font-medium">รับแมวได้สูงสุด:</span>
              <p class="font-semibold text-slate-800">{{ selectedSitter.profile?.max_cats_per_day }} ตัวต่อวัน</p>
            </div>
            <div class="sm:col-span-2">
              <span class="text-xs text-slate-400 font-medium">ที่อยู่สถานที่ดูแล:</span>
              <p class="font-medium text-slate-800 mt-0.5">{{ selectedSitter.profile?.address || 'ไม่ระบุที่อยู่' }}</p>
            </div>
            <div class="sm:col-span-2">
              <span class="text-xs text-slate-400 font-medium">คำแนะนำตัว (Bio):</span>
              <p class="font-normal text-slate-600 mt-0.5">{{ selectedSitter.profile?.bio || '-' }}</p>
            </div>
          </div>

          <!-- ID Card Section -->
          <div>
            <h4 class="font-bold text-slate-800 text-sm mb-2 flex items-center gap-2">
              <FileText class="w-4 h-4 text-[#FF7A59]" /> รูปถ่ายบัตรประชาชน / เอกสารยืนยันตัวตน
            </h4>
            <div v-if="selectedSitter.profile?.id_card_url" class="border border-slate-200 rounded-2xl overflow-hidden bg-slate-100 p-3 flex flex-col items-center justify-center">
              <a :href="resolveImageUrl(selectedSitter.profile.id_card_url)" target="_blank" rel="noopener" class="block max-h-64 overflow-hidden rounded-xl border border-slate-200 bg-white hover:opacity-95 transition">
                <img :src="resolveImageUrl(selectedSitter.profile.id_card_url)" class="max-h-64 object-contain w-full" />
              </a>
              <a
                :href="resolveImageUrl(selectedSitter.profile.id_card_url)"
                target="_blank"
                rel="noopener"
                class="inline-flex items-center gap-1.5 text-xs text-[#FF7A59] hover:underline font-semibold mt-2.5"
              >
                <ExternalLink class="w-3.5 h-3.5" /> คลิกเปิดดูภาพเอกสารขนาดเต็ม
              </a>
            </div>
            <div v-else class="p-6 text-center bg-slate-50 rounded-2xl border border-dashed border-slate-200 text-slate-400 text-xs">
              ไม่ได้แนบรูปถ่ายบัตรประชาชน
            </div>
          </div>

          <!-- Service Place Photos -->
          <div>
            <h4 class="font-bold text-slate-800 text-sm mb-2 flex items-center gap-2">
              <MapPin class="w-4 h-4 text-[#FF7A59]" /> รูปภาพสถานที่ให้บริการดูแลแมว
            </h4>
            <div v-if="selectedSitter.profile?.service_photos && selectedSitter.profile.service_photos.length > 0" class="grid grid-cols-2 sm:grid-cols-3 gap-3">
              <div v-for="(photo, idx) in selectedSitter.profile.service_photos" :key="idx" class="rounded-xl overflow-hidden aspect-video bg-slate-100 border border-slate-200">
                <img :src="resolveImageUrl(photo)" class="w-full h-full object-cover" />
              </div>
            </div>
            <div v-else class="p-4 text-center bg-slate-50 rounded-2xl border border-dashed border-slate-200 text-slate-400 text-xs">
              ไม่ได้แนบรูปถ่ายสถานที่
            </div>
          </div>
        </div>

        <!-- Modal Footer Actions -->
        <div class="px-6 py-4 bg-slate-50 border-t border-slate-100 flex items-center justify-end gap-3">
          <button
            @click="isModalOpen = false"
            class="px-4 py-2.5 rounded-xl border border-slate-200 text-slate-600 font-semibold text-xs hover:bg-slate-100 transition-colors cursor-pointer"
          >
            ปิดหน้าต่าง
          </button>
          <button
            @click="openRejectPrompt"
            class="px-4 py-2.5 rounded-xl bg-rose-50 hover:bg-rose-100 text-rose-600 font-semibold text-xs transition-colors flex items-center gap-1.5 cursor-pointer"
          >
            <X class="w-4 h-4" /> ปฏิเสธ
          </button>
          <button
            @click="approveSitter(selectedSitter)"
            :disabled="isProcessing"
            class="px-6 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-xs transition-all shadow-md shadow-[#FF7A59]/30 flex items-center gap-1.5 cursor-pointer disabled:opacity-50"
          >
            <Loader2 v-if="isProcessing" class="w-4 h-4 animate-spin" />
            <Check v-else class="w-4 h-4" /> อนุมัติผู้รับเลี้ยง
          </button>
        </div>
      </div>
    </div>

    <!-- Reject Reason Submodal -->
    <div
      v-if="isRejectModalOpen"
      class="fixed inset-0 bg-slate-900/60 backdrop-blur-xs flex items-center justify-center p-4 z-60"
    >
      <div class="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-100">
        <h3 class="font-bold text-slate-800 text-base mb-2">ระบุเหตุผลในการปฏิเสธ</h3>
        <p class="text-xs text-slate-500 mb-4">เหตุผลนี้จะถูกส่งแจ้งเตือนไปยังผู้สมัครโดยอัตโนมัติ</p>
        <textarea
          v-model="rejectReason"
          rows="3"
          placeholder="เช่น รูปบัตรประชาชนไม่ชัดเจน, ข้อมูลสถานที่ดูแลไม่ครบถ้วน"
          class="w-full p-3 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-rose-500 mb-4"
        ></textarea>
        <div class="flex items-center justify-end gap-3">
          <button @click="isRejectModalOpen = false" class="px-4 py-2 rounded-lg text-slate-500 hover:bg-slate-100 text-xs font-semibold">
            ยกเลิก
          </button>
          <button @click="confirmReject" :disabled="isProcessing" class="px-5 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white text-xs font-semibold">
            ยืนยันปฏิเสธ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
