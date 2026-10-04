<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { Cat, Lock, Mail, AlertCircle, Loader2, ShieldCheck } from 'lucide-vue-next';

const authStore = useAuthStore();
const router = useRouter();

const email = ref('');
const password = ref('');
const isSubmitting = ref(false);
const errorMessage = ref('');

async function handleSubmit() {
  if (!email.value || !password.value) {
    errorMessage.value = 'กรุณากรอกอีเมลและรหัสผ่าน';
    return;
  }

  isSubmitting.value = true;
  errorMessage.value = '';

  const success = await authStore.login(email.value, password.value);
  isSubmitting.value = false;

  if (success) {
    router.push('/dashboard');
  } else {
    errorMessage.value = authStore.error || 'เข้าสู่ระบบไม่สำเร็จ';
  }
}
</script>

<template>
  <div class="min-h-screen bg-[#1E2238] flex items-center justify-center p-4 relative overflow-hidden font-['Prompt',sans-serif]">
    <!-- Background Decor Glows -->
    <div class="absolute -top-40 -left-40 w-96 h-96 bg-[#FF7A59]/20 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-[#FFC857]/15 rounded-full blur-3xl pointer-events-none"></div>

    <!-- Login Card -->
    <div class="w-full max-w-md bg-white rounded-3xl p-8 shadow-2xl relative z-10 border border-slate-100">
      <!-- Brand Header -->
      <div class="text-center mb-8">
        <div class="w-16 h-16 rounded-2xl bg-gradient-to-tr from-[#FF7A59] to-[#FF9E85] mx-auto flex items-center justify-center shadow-xl shadow-[#FF7A59]/30 mb-4">
          <Cat class="w-9 h-9 text-white" />
        </div>
        <h1 class="text-2xl font-bold text-slate-800">Cat Sitter Admin</h1>
        <p class="text-sm text-slate-500 mt-1 flex items-center justify-center gap-1.5 font-medium">
          <ShieldCheck class="w-4 h-4 text-[#FF7A59]" /> ระบบจัดการสำหรับผู้ดูแลระบบ
        </p>
      </div>

      <!-- Error Alert -->
      <div
        v-if="errorMessage"
        class="mb-6 p-4 rounded-xl bg-red-50 border border-red-200 text-sm text-red-600 flex items-start gap-3"
      >
        <AlertCircle class="w-5 h-5 shrink-0 text-red-500 mt-0.5" />
        <span>{{ errorMessage }}</span>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleSubmit" class="space-y-5">
        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-2">
            อีเมลผู้ดูแลระบบ (Admin Email)
          </label>
          <div class="relative">
            <Mail class="w-5 h-5 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
            <input
              v-model="email"
              type="email"
              required
              placeholder="admin@catsitter.com"
              class="w-full pl-11 pr-4 py-3.5 rounded-xl border border-slate-200 bg-slate-50/50 text-slate-800 placeholder-slate-400 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59] focus:bg-white transition-all"
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold uppercase tracking-wider text-slate-600 mb-2">
            รหัสผ่าน (Password)
          </label>
          <div class="relative">
            <Lock class="w-5 h-5 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
            <input
              v-model="password"
              type="password"
              required
              placeholder="••••••••"
              class="w-full pl-11 pr-4 py-3.5 rounded-xl border border-slate-200 bg-slate-50/50 text-slate-800 placeholder-slate-400 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59] focus:bg-white transition-all"
            />
          </div>
        </div>

        <button
          type="submit"
          :disabled="isSubmitting"
          class="w-full py-4 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-base shadow-lg shadow-[#FF7A59]/30 transition-all duration-200 flex items-center justify-center gap-2 cursor-pointer disabled:opacity-70 disabled:cursor-not-allowed"
        >
          <Loader2 v-if="isSubmitting" class="w-5 h-5 animate-spin" />
          <span>{{ isSubmitting ? 'กำลังตรวจสอบสิทธิ์...' : 'เข้าสู่ระบบผู้ดูแล' }}</span>
        </button>
      </form>

      <!-- Footer Note -->
      <div class="mt-8 text-center pt-6 border-t border-slate-100">
        <p class="text-xs text-slate-400">
          สงวนสิทธิ์เฉพาะผู้ดูแลระบบที่ได้รับอนุญาตเท่านั้น
        </p>
      </div>
    </div>
  </div>
</template>
