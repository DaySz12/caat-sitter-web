<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Loader2, CheckCircle2, MapPin } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { sittersApi } from '@/features/sitters/sitters.api';
import { api } from '@/shared/lib/api';

const auth = useAuthStore();

const bio = ref('');
const pricePerDay = ref(300);
const address = ref('');
const latitude = ref<number | null>(null);
const longitude = ref<number | null>(null);
const experienceYears = ref(1);
const maxCatsPerDay = ref(3);
const isAvailable = ref(true);
const socialLine = ref('');
const socialFacebook = ref('');
const socialInstagram = ref('');
const servicePhotos = ref<string[]>([]);

const isLoading = ref(true);
const isSaving = ref(false);
const isUploadingPhoto = ref(false);
const savedMessage = ref('');
const isLocating = ref(false);

onMounted(async () => {
  if (!auth.user) return;
  const profile = await sittersApi.getById(auth.user.id);
  bio.value = profile.bio ?? '';
  pricePerDay.value = profile.pricePerDay;
  address.value = profile.address ?? '';
  latitude.value = profile.latitude;
  longitude.value = profile.longitude;
  experienceYears.value = profile.experienceYears;
  maxCatsPerDay.value = profile.maxCatsPerDay;
  isAvailable.value = profile.isAvailable;
  socialLine.value = profile.socialLine ?? '';
  socialFacebook.value = profile.socialFacebook ?? '';
  socialInstagram.value = profile.socialInstagram ?? '';
  servicePhotos.value = profile.servicePhotos;
  isLoading.value = false;
});

function useMyLocation() {
  if (!navigator.geolocation) return;
  isLocating.value = true;
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      latitude.value = pos.coords.latitude;
      longitude.value = pos.coords.longitude;
      isLocating.value = false;
    },
    () => {
      isLocating.value = false;
    }
  );
}

async function handlePhotoAdd(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  isUploadingPhoto.value = true;
  try {
    const { url } = await api.upload('sitter-documents', file);
    servicePhotos.value = [...servicePhotos.value, url];
  } finally {
    isUploadingPhoto.value = false;
  }
}

function removePhoto(index: number) {
  servicePhotos.value = servicePhotos.value.filter((_, i) => i !== index);
}

async function handleSave() {
  isSaving.value = true;
  savedMessage.value = '';
  try {
    await sittersApi.updateMine({
      bio: bio.value,
      pricePerDay: pricePerDay.value,
      address: address.value,
      latitude: latitude.value ?? undefined,
      longitude: longitude.value ?? undefined,
      experienceYears: experienceYears.value,
      maxCatsPerDay: maxCatsPerDay.value,
      servicePhotos: servicePhotos.value,
      isAvailable: isAvailable.value,
      socialLine: socialLine.value,
      socialFacebook: socialFacebook.value,
      socialInstagram: socialInstagram.value,
    });
    savedMessage.value = 'บันทึกข้อมูลเรียบร้อยแล้ว';
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <div class="max-w-lg mx-auto bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
    <h1 class="text-lg font-bold text-slate-800 mb-1">โปรไฟล์ผู้รับฝากเลี้ยง</h1>
    <p class="text-xs text-slate-500 mb-5">ข้อมูลนี้จะแสดงให้เจ้าของแมวเห็นเมื่อค้นหา</p>

    <div v-if="isLoading" class="text-center py-10 text-slate-400 text-sm">กำลังโหลด...</div>

    <div v-else class="space-y-4">
      <div v-if="savedMessage" class="p-3 rounded-xl bg-green-50 border border-green-200 text-sm text-green-600 flex items-center gap-2">
        <CheckCircle2 class="w-4 h-4" /> {{ savedMessage }}
      </div>

      <label class="flex items-center justify-between p-3 rounded-xl border border-slate-200">
        <span class="text-sm font-medium text-slate-700">เปิดรับงานอยู่ในขณะนี้</span>
        <input v-model="isAvailable" type="checkbox" class="w-5 h-5 rounded text-[#FF7A59]" />
      </label>

      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">แนะนำตัว (Bio)</label>
        <textarea v-model="bio" rows="3" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm"></textarea>
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">ราคา/วัน (บาท)</label>
          <input v-model.number="pricePerDay" type="number" min="0" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
        </div>
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">ประสบการณ์ (ปี)</label>
          <input v-model.number="experienceYears" type="number" min="0" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
        </div>
      </div>

      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">รับดูแลสูงสุด (ตัว/วัน)</label>
        <input v-model.number="maxCatsPerDay" type="number" min="1" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
      </div>

      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">ที่อยู่ / ย่าน</label>
        <input v-model="address" type="text" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
        <button type="button" class="flex items-center gap-1 mt-2 text-xs text-[#FF7A59] font-semibold" @click="useMyLocation">
          <MapPin class="w-3.5 h-3.5" />
          {{ isLocating ? 'กำลังค้นหาตำแหน่ง...' : latitude ? 'อัปเดตพิกัด GPS แล้ว' : 'ปักหมุดตำแหน่งปัจจุบัน' }}
        </button>
      </div>

      <div class="grid grid-cols-3 gap-3">
        <input v-model="socialLine" type="text" placeholder="Line ID" class="px-3 py-2.5 rounded-xl border border-slate-200 text-xs" />
        <input v-model="socialFacebook" type="text" placeholder="Facebook" class="px-3 py-2.5 rounded-xl border border-slate-200 text-xs" />
        <input v-model="socialInstagram" type="text" placeholder="Instagram" class="px-3 py-2.5 rounded-xl border border-slate-200 text-xs" />
      </div>

      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">รูปผลงาน</label>
        <div class="flex flex-wrap gap-2">
          <div v-for="(photo, i) in servicePhotos" :key="i" class="relative w-16 h-16">
            <img :src="photo" class="w-full h-full object-cover rounded-lg" />
            <button class="absolute -top-1.5 -right-1.5 w-5 h-5 rounded-full bg-red-500 text-white text-xs" @click="removePhoto(i)">×</button>
          </div>
          <label class="w-16 h-16 rounded-lg border-2 border-dashed border-slate-200 flex items-center justify-center cursor-pointer text-slate-400 text-xs">
            <Loader2 v-if="isUploadingPhoto" class="w-4 h-4 animate-spin" />
            <span v-else>+ เพิ่ม</span>
            <input type="file" accept="image/*" class="hidden" @change="handlePhotoAdd" />
          </label>
        </div>
      </div>

      <button
        :disabled="isSaving"
        class="w-full py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm flex items-center justify-center gap-2 disabled:opacity-60"
        @click="handleSave"
      >
        <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
        บันทึกการเปลี่ยนแปลง
      </button>
    </div>
  </div>
</template>
