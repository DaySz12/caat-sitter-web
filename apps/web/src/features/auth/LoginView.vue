<script setup lang="ts">
import { ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useAuthStore } from './auth.store';
import { PawPrint, Mail, Lock, Eye, EyeOff, AlertCircle, Loader2 } from 'lucide-vue-next';

const auth = useAuthStore();
const router = useRouter();

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const rememberMe = ref(true);
const isSubmitting = ref(false);
const errorMessage = ref('');

function fillDemo(role: 'owner' | 'sitter') {
  errorMessage.value = '';
  if (role === 'owner') {
    email.value = 'owner@test.com';
    password.value = 'password123';
  } else {
    email.value = 'sitter@test.com';
    password.value = 'password123';
  }
}

async function handleSubmit() {
  if (!email.value || !password.value) {
    errorMessage.value = 'กรุณากรอกอีเมลและรหัสผ่านให้ครบถ้วน';
    return;
  }
  isSubmitting.value = true;
  errorMessage.value = '';

  const success = await auth.login(email.value, password.value);
  isSubmitting.value = false;

  if (success) {
    router.push(auth.user?.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'อีเมลหรือรหัสผ่านไม่ถูกต้อง';
  }
}

function handleForgotPassword() {
  window.alert('กรุณาติดต่อผู้ดูแลระบบเพื่อรีเซ็ตรหัสผ่าน');
}
</script>

<template>
  <div class="min-h-screen bg-slate-100/70 flex items-center justify-center p-4 sm:p-6">
    <!-- Compact Split Card Container (Fits comfortably on any screen without huge zoom) -->
    <div
      class="w-full max-w-4xl bg-white rounded-3xl shadow-xl border border-slate-200/70 overflow-hidden flex flex-col md:flex-row my-auto"
    >
      <!-- Left: Natural Photo (Frame fits nicely beside the form) -->
      <div class="hidden md:block md:w-1/2 relative bg-slate-100 select-none min-h-[500px]">
        <img
          src="/images/login-hero.jpg"
          alt="Cat Sitter"
          class="absolute inset-0 w-full h-full object-cover object-center"
        />
      </div>

      <!-- Right: Compact Clean Login Form -->
      <div class="w-full md:w-1/2 p-6 sm:p-8 lg:p-9 flex flex-col justify-between">
        <div>
          <!-- Brand Header -->
          <div class="flex items-center gap-2 mb-5">
            <div class="w-8 h-8 rounded-xl bg-[#FF7A59] flex items-center justify-center text-white shadow-xs">
              <PawPrint class="w-4 h-4" />
            </div>
            <span class="text-lg font-bold text-slate-800 tracking-tight">Cat Sitter</span>
          </div>

          <!-- Title -->
          <div class="mb-4">
            <h1 class="text-xl font-bold text-slate-900 tracking-tight">เข้าสู่ระบบ</h1>
            <p class="text-xs text-slate-500 mt-0.5">ยินดีต้อนรับกลับมา กรุณากรอกข้อมูลบัญชีของคุณ</p>
          </div>

          <!-- Error Alert -->
          <div
            v-if="errorMessage"
            class="mb-3.5 p-2.5 rounded-xl bg-red-50 border border-red-200 text-xs text-red-600 flex items-start gap-2"
          >
            <AlertCircle class="w-3.5 h-3.5 text-red-500 shrink-0 mt-0.5" />
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Form Inputs -->
          <form class="space-y-3" @submit.prevent="handleSubmit">
            <div>
              <label class="block text-xs font-medium text-slate-700 mb-1">อีเมล</label>
              <div class="relative">
                <Mail class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="email"
                  type="email"
                  required
                  autocomplete="email"
                  placeholder="you@example.com"
                  class="w-full pl-9 pr-3 py-2 rounded-xl border border-slate-200 text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
              </div>
            </div>

            <div>
              <div class="flex items-center justify-between mb-1">
                <label class="block text-xs font-medium text-slate-700">รหัสผ่าน</label>
                <button
                  type="button"
                  @click="handleForgotPassword"
                  class="text-xs text-[#FF7A59] hover:underline cursor-pointer"
                >
                  ลืมรหัสผ่าน?
                </button>
              </div>
              <div class="relative">
                <Lock class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  required
                  autocomplete="current-password"
                  placeholder="••••••••"
                  class="w-full pl-9 pr-9 py-2 rounded-xl border border-slate-200 text-sm text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
                <button
                  type="button"
                  @click="showPassword = !showPassword"
                  class="absolute right-2.5 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 p-1 cursor-pointer"
                  tabindex="-1"
                >
                  <EyeOff v-if="showPassword" class="w-3.5 h-3.5" />
                  <Eye v-else class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>

            <div class="flex items-center pt-0.5">
              <label class="flex items-center gap-2 text-xs text-slate-600 cursor-pointer">
                <input
                  v-model="rememberMe"
                  type="checkbox"
                  class="w-3.5 h-3.5 rounded border-slate-300 text-[#FF7A59] focus:ring-[#FF7A59]"
                />
                <span>จดจำการเข้าสู่ระบบ</span>
              </label>
            </div>

            <button
              type="submit"
              :disabled="isSubmitting"
              class="w-full py-2.5 px-4 rounded-xl bg-[#FF7A59] hover:bg-[#e86645] text-white font-medium text-sm transition disabled:opacity-50 cursor-pointer flex items-center justify-center gap-2 shadow-xs mt-1"
            >
              <Loader2 v-if="isSubmitting" class="w-3.5 h-3.5 animate-spin" />
              <span>{{ isSubmitting ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ' }}</span>
            </button>
          </form>

          <!-- Subtle Demo accounts fill -->
          <div class="mt-4 pt-3.5 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500">
            <span>ทดสอบระบบ:</span>
            <div class="flex items-center gap-1.5">
              <button
                type="button"
                @click="fillDemo('owner')"
                class="px-2 py-0.5 rounded-md bg-slate-100 hover:bg-slate-200 text-slate-700 transition cursor-pointer font-medium"
              >
                เจ้าของแมว
              </button>
              <button
                type="button"
                @click="fillDemo('sitter')"
                class="px-2 py-0.5 rounded-md bg-slate-100 hover:bg-slate-200 text-slate-700 transition cursor-pointer font-medium"
              >
                พี่เลี้ยง
              </button>
            </div>
          </div>
        </div>

        <!-- Footer Signup Link -->
        <div class="mt-5 pt-3 border-t border-slate-100 text-center text-xs text-slate-500">
          <span>ยังไม่มีบัญชี? </span>
          <RouterLink to="/signup/owner" class="text-[#FF7A59] hover:underline font-medium">สมัครเป็นเจ้าของแมว</RouterLink>
          <span class="mx-1.5 text-slate-300">·</span>
          <RouterLink to="/signup/sitter" class="text-[#FF7A59] hover:underline font-medium">สมัครเป็นผู้รับฝากเลี้ยง</RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>
