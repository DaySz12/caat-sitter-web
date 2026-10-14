<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Cat as CatIcon, Loader2, ArrowLeft } from 'lucide-vue-next';
import { catsApi } from './cats.api';
import { api } from '@/shared/lib/api';

const route = useRoute();
const router = useRouter();
const catId = computed(() => route.params.id as string | undefined);
const isEdit = computed(() => !!catId.value);

const name = ref('');
const breed = ref('');
const ageYears = ref(1);
const ageMonths = ref(0);
const weight = ref<number | null>(null);
const gender = ref<'male' | 'female'>('male');
const isNeutered = ref(false);
const photoUrl = ref<string | null>(null);
const dietaryHabits = ref('');
const medicalNotes = ref('');
const emergencyVetClinic = ref('');
const emergencyContactPhone = ref('');

const isSaving = ref(false);
const isUploading = ref(false);
const errorMessage = ref('');

onMounted(async () => {
  if (!catId.value) return;
  const cat = await catsApi.getById(catId.value);
  name.value = cat.name;
  breed.value = cat.breed ?? '';
  ageYears.value = cat.ageYears;
  ageMonths.value = cat.ageMonths;
  weight.value = cat.weight;
  gender.value = cat.gender;
  isNeutered.value = cat.isNeutered;
  photoUrl.value = cat.photoUrl;
  dietaryHabits.value = cat.dietaryHabits ?? '';
  medicalNotes.value = cat.medicalNotes ?? '';
  emergencyVetClinic.value = cat.emergencyVetClinic ?? '';
  emergencyContactPhone.value = cat.emergencyContactPhone ?? '';
});

async function handlePhotoChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  isUploading.value = true;
  try {
    const { url } = await api.upload('cat-images', file);
    photoUrl.value = url;
  } catch (err: any) {
    errorMessage.value = err.message || 'อัปโหลดรูปไม่สำเร็จ';
  } finally {
    isUploading.value = false;
  }
}

async function handleSubmit() {
  if (!name.value.trim()) {
    errorMessage.value = 'กรุณากรอกชื่อแมว';
    return;
  }
  isSaving.value = true;
  errorMessage.value = '';
  try {
    const payload = {
      name: name.value.trim(),
      breed: breed.value || null,
      ageYears: ageYears.value,
      ageMonths: ageMonths.value,
      weight: weight.value,
      gender: gender.value,
      isNeutered: isNeutered.value,
      photoUrl: photoUrl.value,
      dietaryHabits: dietaryHabits.value || null,
      medicalNotes: medicalNotes.value || null,
      emergencyVetClinic: emergencyVetClinic.value || null,
      emergencyContactPhone: emergencyContactPhone.value || null,
      vaccines: [],
    };
    if (isEdit.value && catId.value) {
      await catsApi.update(catId.value, payload);
    } else {
      await catsApi.create(payload);
    }
    router.push('/owner/cats');
  } catch (err: any) {
    errorMessage.value = err.message || 'บันทึกไม่สำเร็จ';
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <div class="max-w-lg mx-auto">
    <button class="flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-700 mb-4" @click="router.back()">
      <ArrowLeft class="w-4 h-4" /> กลับ
    </button>

    <div class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
      <h1 class="text-lg font-bold text-slate-800 mb-5">{{ isEdit ? 'แก้ไขข้อมูลแมว' : 'เพิ่มแมวใหม่' }}</h1>

      <div v-if="errorMessage" class="mb-4 p-3 rounded-xl bg-red-50 border border-red-200 text-sm text-red-600">
        {{ errorMessage }}
      </div>

      <div class="flex justify-center mb-5">
        <label class="relative cursor-pointer group">
          <div class="w-24 h-24 rounded-2xl bg-[#FFF0EB] overflow-hidden flex items-center justify-center">
            <img v-if="photoUrl" :src="photoUrl" class="w-full h-full object-cover" />
            <CatIcon v-else class="w-10 h-10 text-[#FF7A59]" />
          </div>
          <div class="absolute inset-0 rounded-2xl bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center text-white text-xs transition">
            <Loader2 v-if="isUploading" class="w-5 h-5 animate-spin" />
            <span v-else>เปลี่ยนรูป</span>
          </div>
          <input type="file" accept="image/*" class="hidden" @change="handlePhotoChange" />
        </label>
      </div>

      <form class="space-y-4" @submit.prevent="handleSubmit">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">ชื่อแมว</label>
          <input v-model="name" type="text" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">สายพันธุ์</label>
          <input v-model="breed" type="text" placeholder="เช่น เปอร์เซีย, ไทย" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1.5">อายุ (ปี)</label>
            <input v-model.number="ageYears" type="number" min="0" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1.5">อายุ (เดือน)</label>
            <input v-model.number="ageMonths" type="number" min="0" max="11" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
          </div>
        </div>
        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1.5">น้ำหนัก (กก.)</label>
            <input v-model.number="weight" type="number" step="0.1" min="0" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
          </div>
          <div>
            <label class="block text-sm font-medium text-slate-700 mb-1.5">เพศ</label>
            <select v-model="gender" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm">
              <option value="male">เพศผู้</option>
              <option value="female">เพศเมีย</option>
            </select>
          </div>
        </div>
        <label class="flex items-center gap-2 text-sm text-slate-600">
          <input v-model="isNeutered" type="checkbox" class="rounded border-slate-300 text-[#FF7A59] focus:ring-[#FF7A59]" />
          ทำหมันแล้ว
        </label>
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">พฤติกรรมการกิน / อาหาร</label>
          <textarea v-model="dietaryHabits" rows="2" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm"></textarea>
        </div>
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">ประวัติสุขภาพ / ยาที่ต้องกิน</label>
          <textarea v-model="medicalNotes" rows="2" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm"></textarea>
        </div>

        <div class="border-t border-slate-100 pt-3">
          <p class="text-xs font-semibold text-slate-500 mb-2">🚨 ข้อมูลกรณีฉุกเฉิน (ไม่บังคับ)</p>
          <div class="space-y-3">
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1.5">คลินิก / โรงพยาบาลสัตว์ประจำตัว</label>
              <input v-model="emergencyVetClinic" type="text" placeholder="เช่น รพ.สัตว์ทองหล่อ (สาขาสุขุมวิท)" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
            </div>
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1.5">เบอร์ติดต่อฉุกเฉินสำรอง</label>
              <input v-model="emergencyContactPhone" type="tel" placeholder="เช่น 089-123-4567 (คุณแม่)" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
            </div>
          </div>
        </div>

        <button
          type="submit"
          :disabled="isSaving"
          class="w-full py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm flex items-center justify-center gap-2 disabled:opacity-60"
        >
          <Loader2 v-if="isSaving" class="w-4 h-4 animate-spin" />
          บันทึก
        </button>
      </form>
    </div>
  </div>
</template>
