<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api, API_URL } from '../api/client';
import {
  Image as ImageIcon,
  Plus,
  Trash2,
  Edit2,
  ExternalLink,
  Upload,
  CheckCircle2,
  AlertCircle,
  Eye,
  Layers,
  Sparkles,
  X,
  Loader2
} from 'lucide-vue-next';

interface BannerItem {
  id: string;
  type: 'banner' | 'popup';
  title: string;
  image_url: string;
  link_url?: string | null;
  is_active: boolean;
  display_order: number;
  created_at: string;
}

const banners = ref<BannerItem[]>([]);
const isLoading = ref(true);
const activeTab = ref<'all' | 'banner' | 'popup'>('all');

// Modal state
const isModalOpen = ref(false);
const isEditing = ref(false);
const isSaving = ref(false);
const isUploading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

// Form state
const currentId = ref<string | null>(null);
const formType = ref<'banner' | 'popup'>('banner');
const formTitle = ref('');
const formImageUrl = ref('');
const formLinkUrl = ref('');
const formDisplayOrder = ref(1);
const formIsActive = ref(true);
const fileInput = ref<HTMLInputElement | null>(null);

async function loadBanners() {
  isLoading.value = true;
  try {
    banners.value = await api.get<BannerItem[]>('/api/admin/banners');
  } catch (e: any) {
    console.error('Error loading banners:', e);
    errorMessage.value = e.message || 'โหลดข้อมูลแบนเนอร์ไม่สำเร็จ';
  } finally {
    isLoading.value = false;
  }
}

const filteredBanners = computed(() => {
  if (activeTab.value === 'all') return banners.value;
  return banners.value.filter((b) => b.type === activeTab.value);
});

function openCreateModal(type: 'banner' | 'popup' = 'banner') {
  isEditing.value = false;
  currentId.value = null;
  formType.value = type;
  formTitle.value = '';
  formImageUrl.value = '';
  formLinkUrl.value = '';
  formDisplayOrder.value = banners.value.length + 1;
  formIsActive.value = true;
  errorMessage.value = '';
  isModalOpen.value = true;
}

function openEditModal(item: BannerItem) {
  isEditing.value = true;
  currentId.value = item.id;
  formType.value = item.type;
  formTitle.value = item.title;
  formImageUrl.value = item.image_url;
  formLinkUrl.value = item.link_url || '';
  formDisplayOrder.value = item.display_order;
  formIsActive.value = item.is_active;
  errorMessage.value = '';
  isModalOpen.value = true;
}

function resolveImageUrl(url: string) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  return `${API_URL}${url}`;
}

async function handleFileUpload(event: Event) {
  const target = event.target as HTMLInputElement;
  if (!target.files || target.files.length === 0) return;

  const file = target.files[0];
  isUploading.value = true;
  errorMessage.value = '';

  try {
    const res = await api.upload('banners', file);
    formImageUrl.value = res.url;
  } catch (e: any) {
    errorMessage.value = 'อัปโหลดรูปภาพล้มเหลว: ' + (e.message || 'กรุณาลองใหม่อีกครั้ง');
  } finally {
    isUploading.value = false;
    if (fileInput.value) fileInput.value.value = '';
  }
}

async function handleSave() {
  if (!formTitle.value.trim()) {
    errorMessage.value = 'กรุณาระบุชื่อหรือหัวข้อ';
    return;
  }
  if (!formImageUrl.value.trim()) {
    errorMessage.value = 'กรุณาอัปโหลดรูปภาพหรือใส่ URL รูปภาพ';
    return;
  }

  isSaving.value = true;
  errorMessage.value = '';

  try {
    const payload = {
      type: formType.value,
      title: formTitle.value.trim(),
      image_url: formImageUrl.value.trim(),
      link_url: formLinkUrl.value.trim() || null,
      display_order: Number(formDisplayOrder.value) || 0,
      is_active: formIsActive.value,
    };

    if (isEditing.value && currentId.value) {
      const updated = await api.put<BannerItem>(`/api/admin/banners/${currentId.value}`, payload);
      const index = banners.value.findIndex((b) => b.id === currentId.value);
      if (index !== -1) banners.value[index] = updated;
      showSuccess('แก้ไขข้อมูลสำเร็จแล้ว');
    } else {
      const created = await api.post<BannerItem>('/api/admin/banners', payload);
      banners.value.unshift(created);
      showSuccess('เพิ่มแบนเนอร์/ป๊อปอัปใหม่สำเร็จแล้ว');
    }

    isModalOpen.value = false;
  } catch (e: any) {
    errorMessage.value = e.message || 'เกิดข้อผิดพลาดในการบันทึกข้อมูล';
  } finally {
    isSaving.value = false;
  }
}

async function toggleActive(item: BannerItem) {
  const originalState = item.is_active;
  item.is_active = !originalState;

  try {
    await api.patch(`/api/admin/banners/${item.id}/toggle`);
    showSuccess(`${item.type === 'banner' ? 'แบนเนอร์' : 'ป๊อปอัป'} "${item.title}" ${item.is_active ? 'เปิดใช้งานแล้ว' : 'ปิดการแสดงผลแล้ว'}`);
  } catch (e: any) {
    item.is_active = originalState;
    console.error('Failed to toggle banner active state', e);
  }
}

async function handleDelete(item: BannerItem) {
  if (!confirm(`คุณแน่ใจหรือไม่ว่าต้องการลบ "${item.title}"?`)) return;

  try {
    await api.delete(`/api/admin/banners/${item.id}`);
    banners.value = banners.value.filter((b) => b.id !== item.id);
    showSuccess('ลบรายการสำเร็จแล้ว');
  } catch (e: any) {
    alert('ลบไม่สำเร็จ: ' + (e.message || 'เกิดข้อผิดพลาด'));
  }
}

function showSuccess(msg: string) {
  successMessage.value = msg;
  setTimeout(() => {
    successMessage.value = '';
  }, 3000);
}

onMounted(() => {
  loadBanners();
});
</script>

<template>
  <div class="space-y-6 max-w-7xl">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800 flex items-center gap-2">
          <ImageIcon class="w-7 h-7 text-[#FF7A59]" />
          จัดการแบนเนอร์ & ป๊อปอัป (Banners & Popups)
        </h1>
        <p class="text-xs text-slate-500 mt-1">
          กำหนดรูปภาพโปรโมชั่นสำหรับ Slider และ Popup แจ้งเตือนที่จะแสดงบนหน้าเว็บของลูกค้า
        </p>
      </div>

      <div class="flex items-center gap-2">
        <button
          @click="openCreateModal('banner')"
          class="inline-flex items-center gap-1.5 px-4 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white text-sm font-semibold transition-all shadow-md shadow-[#FF7A59]/20 cursor-pointer"
        >
          <Plus class="w-4 h-4" />
          เพิ่มแบนเนอร์สไลด์
        </button>
        <button
          @click="openCreateModal('popup')"
          class="inline-flex items-center gap-1.5 px-4 py-2.5 rounded-xl bg-[#2E3456] hover:bg-[#1E2238] text-white text-sm font-semibold transition-all shadow-md shadow-slate-900/10 cursor-pointer"
        >
          <Sparkles class="w-4 h-4 text-amber-300" />
          เพิ่มป๊อปอัป
        </button>
      </div>
    </div>

    <!-- Success Toast -->
    <div
      v-if="successMessage"
      class="p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-sm text-emerald-700 flex items-center gap-3 transition-all"
    >
      <CheckCircle2 class="w-5 h-5 text-emerald-600 shrink-0" />
      <span>{{ successMessage }}</span>
    </div>

    <!-- Tab Filter -->
    <div class="flex items-center gap-2 border-b border-slate-200 pb-1">
      <button
        @click="activeTab = 'all'"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all',
          activeTab === 'all'
            ? 'bg-white text-slate-800 shadow-xs border border-slate-200/80 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        ทั้งหมด ({{ banners.length }})
      </button>
      <button
        @click="activeTab = 'banner'"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-1.5',
          activeTab === 'banner'
            ? 'bg-white text-[#FF7A59] shadow-xs border border-slate-200/80 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <Layers class="w-4 h-4" />
        แบนเนอร์ Slider ({{ banners.filter((b) => b.type === 'banner').length }})
      </button>
      <button
        @click="activeTab = 'popup'"
        :class="[
          'px-4 py-2 text-sm font-medium rounded-lg transition-all flex items-center gap-1.5',
          activeTab === 'popup'
            ? 'bg-white text-purple-600 shadow-xs border border-slate-200/80 font-semibold'
            : 'text-slate-500 hover:text-slate-800'
        ]"
      >
        <Sparkles class="w-4 h-4" />
        ป๊อปอัปแจ้งเตือน ({{ banners.filter((b) => b.type === 'popup').length }})
      </button>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="isLoading" class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <div v-for="i in 3" :key="i" class="bg-white rounded-2xl border border-slate-100 p-4 animate-pulse space-y-3">
        <div class="w-full h-44 bg-slate-100 rounded-xl"></div>
        <div class="h-4 bg-slate-100 rounded w-3/4"></div>
        <div class="h-3 bg-slate-100 rounded w-1/2"></div>
      </div>
    </div>

    <!-- Empty State -->
    <div
      v-else-if="filteredBanners.length === 0"
      class="bg-white rounded-2xl border border-slate-200/80 p-12 text-center"
    >
      <div class="w-16 h-16 rounded-2xl bg-orange-50 text-[#FF7A59] flex items-center justify-center mx-auto mb-4">
        <ImageIcon class="w-8 h-8" />
      </div>
      <h3 class="text-base font-semibold text-slate-800">ยังไม่มีข้อมูลในหมวดนี้</h3>
      <p class="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
        คุณสามารถกดปุ่ม "เพิ่มแบนเนอร์" หรือ "เพิ่มป๊อปอัป" ด้านบนเพื่อเริ่มตั้งค่าสื่อประชาสัมพันธ์บนหน้าเว็บได้ทันที
      </p>
    </div>

    <!-- Banners Grid -->
    <div v-else class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <div
        v-for="item in filteredBanners"
        :key="item.id"
        :class="[
          'bg-white rounded-2xl border transition-all duration-200 overflow-hidden flex flex-col group',
          item.is_active ? 'border-slate-200/90 shadow-sm hover:shadow-md' : 'border-slate-200 opacity-60 bg-slate-50/50'
        ]"
      >
        <!-- Thumbnail Preview -->
        <div class="relative w-full h-44 bg-slate-900 overflow-hidden flex items-center justify-center">
          <img
            :src="resolveImageUrl(item.image_url)"
            :alt="item.title"
            class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
            @error="(e) => ((e.target as HTMLImageElement).src = 'https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=600')"
          />

          <!-- Badge Overlay -->
          <div class="absolute top-3 left-3 flex items-center gap-1.5">
            <span
              :class="[
                'px-2.5 py-1 rounded-md text-xs font-semibold backdrop-blur-md shadow-xs',
                item.type === 'banner'
                  ? 'bg-blue-600/90 text-white'
                  : 'bg-purple-600/90 text-white'
              ]"
            >
              {{ item.type === 'banner' ? 'แบนเนอร์ Slider' : 'ป๊อปอัป Modal' }}
            </span>

            <span class="px-2 py-0.5 rounded-md text-[11px] font-medium bg-black/60 text-white backdrop-blur-xs">
              ลำดับ: {{ item.display_order }}
            </span>
          </div>

          <!-- Active status tag -->
          <div class="absolute top-3 right-3">
            <span
              :class="[
                'px-2.5 py-1 rounded-full text-xs font-semibold flex items-center gap-1 shadow-xs',
                item.is_active
                  ? 'bg-emerald-500 text-white'
                  : 'bg-slate-700 text-slate-200'
              ]"
            >
              <span class="w-1.5 h-1.5 rounded-full" :class="item.is_active ? 'bg-white' : 'bg-slate-400'"></span>
              {{ item.is_active ? 'แสดงอยู่' : 'ปิดใช้งาน' }}
            </span>
          </div>
        </div>

        <!-- Content -->
        <div class="p-4 flex-1 flex flex-col justify-between">
          <div>
            <h3 class="font-bold text-slate-800 text-sm line-clamp-1" :title="item.title">
              {{ item.title }}
            </h3>
            <p v-if="item.link_url" class="text-xs text-blue-600 mt-1 flex items-center gap-1 truncate" :title="item.link_url">
              <ExternalLink class="w-3 h-3 shrink-0" />
              {{ item.link_url }}
            </p>
            <p v-else class="text-xs text-slate-400 mt-1 italic">
              ไม่มีการเชื่อมโยงลิงก์
            </p>
          </div>

          <!-- Actions -->
          <div class="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between">
            <label class="relative inline-flex items-center cursor-pointer">
              <input
                type="checkbox"
                :checked="item.is_active"
                @change="toggleActive(item)"
                class="sr-only peer"
              />
              <div
                class="w-9 h-5 bg-slate-300 peer-focus:outline-hidden rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:start-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-4 after:w-4 after:transition-all peer-checked:bg-emerald-500"
              ></div>
              <span class="ms-2 text-xs font-medium text-slate-600">
                {{ item.is_active ? 'เปิด' : 'ปิด' }}
              </span>
            </label>

            <div class="flex items-center gap-1">
              <button
                @click="openEditModal(item)"
                class="p-1.5 text-slate-500 hover:text-[#FF7A59] hover:bg-orange-50 rounded-lg transition-colors cursor-pointer"
                title="แก้ไข"
              >
                <Edit2 class="w-4 h-4" />
              </button>
              <button
                @click="handleDelete(item)"
                class="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors cursor-pointer"
                title="ลบ"
              >
                <Trash2 class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create / Edit Modal -->
    <div
      v-if="isModalOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/50 backdrop-blur-xs"
    >
      <div
        class="bg-white rounded-3xl max-w-lg w-full p-6 shadow-2xl border border-slate-100 relative animate-in fade-in zoom-in-95 duration-200"
      >
        <button
          @click="isModalOpen = false"
          class="absolute top-5 right-5 text-slate-400 hover:text-slate-600 p-1 rounded-lg"
        >
          <X class="w-5 h-5" />
        </button>

        <h2 class="text-lg font-bold text-slate-800 mb-4 flex items-center gap-2">
          <ImageIcon class="w-5 h-5 text-[#FF7A59]" />
          {{ isEditing ? 'แก้ไขข้อมูล' : 'เพิ่มแบนเนอร์ / ป๊อปอัปใหม่' }}
        </h2>

        <!-- Error Alert in Modal -->
        <div
          v-if="errorMessage"
          class="mb-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-xs text-rose-700 flex items-center gap-2"
        >
          <AlertCircle class="w-4 h-4 shrink-0" />
          <span>{{ errorMessage }}</span>
        </div>

        <form @submit.prevent="handleSave" class="space-y-4">
          <!-- Type Select -->
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1.5">ประเภทการแสดงผล</label>
            <div class="grid grid-cols-2 gap-2">
              <button
                type="button"
                @click="formType = 'banner'"
                :class="[
                  'py-2.5 px-3 rounded-xl border text-xs font-semibold flex items-center justify-center gap-1.5 transition-all cursor-pointer',
                  formType === 'banner'
                    ? 'border-[#FF7A59] bg-orange-50/50 text-[#FF7A59]'
                    : 'border-slate-200 text-slate-600 hover:bg-slate-50'
                ]"
              >
                <Layers class="w-4 h-4" />
                แบนเนอร์ Slider หน้าแรก
              </button>
              <button
                type="button"
                @click="formType = 'popup'"
                :class="[
                  'py-2.5 px-3 rounded-xl border text-xs font-semibold flex items-center justify-center gap-1.5 transition-all cursor-pointer',
                  formType === 'popup'
                    ? 'border-purple-600 bg-purple-50 text-purple-700'
                    : 'border-slate-200 text-slate-600 hover:bg-slate-50'
                ]"
              >
                <Sparkles class="w-4 h-4" />
                ป๊อปอัปแจ้งเตือน (Modal)
              </button>
            </div>
          </div>

          <!-- Title -->
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">ชื่อหรือหัวข้อแบนเนอร์ *</label>
            <input
              v-model="formTitle"
              type="text"
              placeholder="เช่น โปรโมชั่นต้อนรับหน้าร้อน ลด 15%"
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-hidden focus:border-[#FF7A59] focus:ring-2 focus:ring-[#FF7A59]/20"
              required
            />
          </div>

          <!-- Image Upload / URL -->
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">รูปภาพ *</label>

            <!-- Image Preview Box -->
            <div
              v-if="formImageUrl"
              class="relative w-full h-36 rounded-xl overflow-hidden mb-2 bg-slate-900 border border-slate-200"
            >
              <img
                :src="resolveImageUrl(formImageUrl)"
                alt="Preview"
                class="w-full h-full object-cover"
              />
              <button
                type="button"
                @click="formImageUrl = ''"
                class="absolute top-2 right-2 p-1 rounded-full bg-black/60 text-white hover:bg-rose-600 transition-colors"
                title="ลบรูป"
              >
                <X class="w-4 h-4" />
              </button>
            </div>

            <!-- Upload Controls -->
            <div class="space-y-2">
              <div class="flex items-center gap-2">
                <input
                  ref="fileInput"
                  type="file"
                  accept="image/*"
                  @change="handleFileUpload"
                  class="hidden"
                />
                <button
                  type="button"
                  @click="fileInput?.click()"
                  :disabled="isUploading"
                  class="inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-slate-100 hover:bg-slate-200 text-slate-700 text-xs font-semibold transition-colors cursor-pointer"
                >
                  <Loader2 v-if="isUploading" class="w-3.5 h-3.5 animate-spin text-[#FF7A59]" />
                  <Upload v-else class="w-3.5 h-3.5" />
                  {{ isUploading ? 'กำลังอัปโหลด...' : 'อัปโหลดรูปจากเครื่อง' }}
                </button>
                <span class="text-xs text-slate-400">หรือวาง Image URL ด้านล่าง:</span>
              </div>

              <input
                v-model="formImageUrl"
                type="text"
                placeholder="https://example.com/banner.jpg หรือ /uploads/..."
                class="w-full px-3.5 py-2 rounded-xl border border-slate-200 text-xs focus:outline-hidden focus:border-[#FF7A59]"
              />
            </div>
            <p class="text-[11px] text-slate-400 mt-1">
              {{ formType === 'banner' ? 'แนะนำขนาด 1200x500 px (สัดส่วนประมาณ 16:9 หรือ 21:9)' : 'แนะนำขนาด 800x800 px หรือ 800x1000 px' }}
            </p>
          </div>

          <!-- Link URL -->
          <div>
            <label class="block text-xs font-semibold text-slate-700 mb-1">ลิงก์ปลายทางเมื่อคลิก (Optional)</label>
            <input
              v-model="formLinkUrl"
              type="text"
              placeholder="เช่น /owner/sitters หรือ https://..."
              class="w-full px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-hidden focus:border-[#FF7A59]"
            />
          </div>

          <!-- Display Order & Active Checkbox -->
          <div class="grid grid-cols-2 gap-4 items-center pt-2">
            <div>
              <label class="block text-xs font-semibold text-slate-700 mb-1">ลำดับการแสดงผล</label>
              <input
                v-model.number="formDisplayOrder"
                type="number"
                min="0"
                class="w-full px-3 py-2 rounded-xl border border-slate-200 text-sm focus:outline-hidden focus:border-[#FF7A59]"
              />
            </div>

            <div class="pt-5">
              <label class="flex items-center gap-2 cursor-pointer">
                <input
                  v-model="formIsActive"
                  type="checkbox"
                  class="w-4 h-4 rounded text-[#FF7A59] focus:ring-[#FF7A59]"
                />
                <span class="text-xs font-semibold text-slate-700">เปิดแสดงผลทันที</span>
              </label>
            </div>
          </div>

          <!-- Submit Buttons -->
          <div class="flex items-center justify-end gap-3 pt-4 border-t border-slate-100">
            <button
              type="button"
              @click="isModalOpen = false"
              class="px-4 py-2 rounded-xl border border-slate-200 text-slate-600 hover:bg-slate-50 text-xs font-semibold"
            >
              ยกเลิก
            </button>
            <button
              type="submit"
              :disabled="isSaving || isUploading"
              class="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white text-xs font-semibold shadow-md shadow-[#FF7A59]/30 transition-all cursor-pointer disabled:opacity-50"
            >
              <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
              <span>{{ isEditing ? 'บันทึกการแก้ไข' : 'สร้างแบนเนอร์' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>
