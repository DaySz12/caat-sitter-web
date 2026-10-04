<script setup lang="ts">
import { ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useAuthStore } from './auth.store';
import { PawPrint, Mail, Lock, AlertCircle, Loader2 } from 'lucide-vue-next';

const auth = useAuthStore();
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

  const success = await auth.login(email.value, password.value);
  isSubmitting.value = false;

  if (success) {
    router.push(auth.user?.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'เข้าสู่ระบบไม่สำเร็จ';
  }
}
</script>

<template>
  <div class="min-h-screen bg-[#1E2238] flex items-center justify-center p-4 relative overflow-hidden">
    <div class="absolute -top-40 -left-40 w-96 h-96 bg-[#FF7A59]/20 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-[#FFC857]/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="w-full max-w-md bg-white rounded-3xl p-8 shadow-2xl relative z-10 border border-slate-100">
      <div class="text-center mb-8">
        <div class="w-16 h-16 rounded-2xl bg-gradient-to-tr from-[#FF7A59] to-[#FF9E85] mx-auto flex items-center justify-center shadow-xl shadow-[#FF7A59]/30 mb-4">
          <PawPrint class="w-9 h-9 text-white" />
        </div>
        <h1 class="text-2xl font-bold text-slate-800">Cat Sitter</h1>
        <p class="text-sm text-slate-500 mt-1">ระบบจองผู้รับฝากเลี้ยงแมว</p>
      </div>

      <div v-if="errorMessage" class="mb-6 p-4 rounded-xl bg-red-50 border border-red-200 text-sm text-red-600 flex items-start gap-3">
        <AlertCircle class="w-5 h-5 shrink-0 text-red-500 mt-0.5" />
        <span>{{ errorMessage }}</span>
      </div>

      <form class="space-y-4" @submit.prevent="handleSubmit">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">อีเมล</label>
          <div class="relative">
            <Mail class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
            <input
              v-model="email"
              type="email"
              placeholder="you@example.com"
              class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]"
            />
          </div>
        </div>
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">รหัสผ่าน</label>
          <div class="relative">
            <Lock class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
            <input
              v-model="password"
              type="password"
              placeholder="••••••••"
              class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]"
            />
          </div>
        </div>

        <button
          type="submit"
          :disabled="isSubmitting"
          class="w-full py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm flex items-center justify-center gap-2 transition disabled:opacity-60"
        >
          <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
          เข้าสู่ระบบ
        </button>
      </form>

      <div class="mt-6 text-center text-sm text-slate-500 space-y-1">
        <p>
          ยังไม่มีบัญชี?
          <RouterLink to="/signup/owner" class="text-[#FF7A59] font-semibold hover:underline">สมัครเป็นเจ้าของแมว</RouterLink>
        </p>
        <p>
          หรือ
          <RouterLink to="/signup/sitter" class="text-[#FF7A59] font-semibold hover:underline">สมัครเป็นผู้รับฝากเลี้ยง</RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>
