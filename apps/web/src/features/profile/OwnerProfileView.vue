<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { User, Loader2, CheckCircle2 } from 'lucide-vue-next';
import { useAuthStore } from '@/features/auth/auth.store';
import { authApi } from '@/features/auth/auth.api';
import { api } from '@/shared/lib/api';

const auth = useAuthStore();
const name = ref('');
const phone = ref('');
const avatarUrl = ref<string | null>(null);
const isSaving = ref(false);
const isUploading = ref(false);
const savedMessage = ref('');

onMounted(() => {
  name.value = auth.user?.name ?? '';
  phone.value = auth.user?.phone ?? '';
  avatarUrl.value = auth.user?.avatarUrl ?? null;
});

async function handleAvatarChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  isUploading.value = true;
  try {
    const { url } = await api.upload('avatars', file);
    avatarUrl.value = url;
  } finally {
    isUploading.value = false;
  }
}

async function handleSave() {
  isSaving.value = true;
  savedMessage.value = '';
  try {
    auth.user = await authApi.updateProfile({ name: name.value, phone: phone.value, avatarUrl: avatarUrl.value ?? undefined });
    savedMessage.value = 'บันทึกข้อมูลเรียบร้อยแล้ว';
  } finally {
    isSaving.value = false;
  }
}
</script>

<template>
  <div class="max-w-lg mx-auto bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
    <h1 class="text-lg font-bold text-slate-800 mb-5">โปรไฟล์ของฉัน</h1>

    <div v-if="savedMessage" class="mb-4 p-3 rounded-xl bg-green-50 border border-green-200 text-sm text-green-600 flex items-center gap-2">
      <CheckCircle2 class="w-4 h-4" /> {{ savedMessage }}
    </div>

    <div class="flex justify-center mb-5">
      <label class="relative cursor-pointer group">
        <div class="w-24 h-24 rounded-2xl bg-[#FFF0EB] overflow-hidden flex items-center justify-center">
          <img v-if="avatarUrl" :src="avatarUrl" class="w-full h-full object-cover" />
          <User v-else class="w-10 h-10 text-[#FF7A59]" />
        </div>
        <div class="absolute inset-0 rounded-2xl bg-black/40 opacity-0 group-hover:opacity-100 flex items-center justify-center text-white text-xs transition">
          <Loader2 v-if="isUploading" class="w-5 h-5 animate-spin" />
          <span v-else>เปลี่ยนรูป</span>
        </div>
        <input type="file" accept="image/*" class="hidden" @change="handleAvatarChange" />
      </label>
    </div>

    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">ชื่อ-นามสกุล</label>
        <input v-model="name" type="text" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
      </div>
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">อีเมล</label>
        <input :value="auth.user?.email" type="email" disabled class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm bg-slate-50 text-slate-400" />
      </div>
      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">เบอร์โทรศัพท์</label>
        <input v-model="phone" type="tel" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm" />
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
