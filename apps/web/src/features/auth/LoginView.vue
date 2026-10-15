<script setup lang="ts">
import { ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useAuthStore } from './auth.store';
import { PawPrint, Mail, Lock, Eye, EyeOff, AlertCircle, Loader2, X, CheckCircle2, User } from 'lucide-vue-next';

const auth = useAuthStore();
const router = useRouter();

const email = ref('');
const password = ref('');
const showPassword = ref(false);
const rememberMe = ref(true);
const isSubmitting = ref(false);
const errorMessage = ref('');

// LINE SSO Modal state
const showLineModal = ref(false);
const lineUserName = ref('คุณเหมียว (LINE Member)');
const lineUserRole = ref<'owner' | 'sitter'>('owner');
const isSubmittingSso = ref(false);

// Google SSO Modal state
const showGoogleModal = ref(false);
const googleUserName = ref('สมชาย ใจดี (Google)');
const googleUserEmail = ref('somchai.google@gmail.com');
const googleUserRole = ref<'owner' | 'sitter'>('owner');

function openGoogleModal() {
  errorMessage.value = '';
  showGoogleModal.value = true;
}

async function handleGoogleSsoLogin() {
  isSubmittingSso.value = true;
  errorMessage.value = '';

  const success = await auth.ssoLogin({
    provider: 'google',
    ssoId: 'GOOGLE_SUB_1092837465',
    name: googleUserName.value.trim() || 'Google User',
    email: googleUserEmail.value.trim() || 'somchai.google@gmail.com',
    role: googleUserRole.value,
    avatarUrl: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=256&q=80',
  });

  isSubmittingSso.value = false;

  if (success) {
    showGoogleModal.value = false;
    router.push(auth.user?.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'เข้าสู่ระบบด้วย Google ไม่สำเร็จ';
    showGoogleModal.value = false;
  }
}

function openLineModal() {
  errorMessage.value = '';
  showLineModal.value = true;
}

async function handleLineSsoLogin() {
  isSubmittingSso.value = true;
  errorMessage.value = '';

  const success = await auth.ssoLogin({
    provider: 'line',
    ssoId: 'LINE_U998877665544',
    name: lineUserName.value.trim() || 'LINE User',
    email: 'line.member@line.me',
    role: lineUserRole.value,
    avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=256&q=80',
  });

  isSubmittingSso.value = false;

  if (success) {
    showLineModal.value = false;
    router.push(auth.user?.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'เข้าสู่ระบบด้วย LINE ไม่สำเร็จ';
    showLineModal.value = false;
  }
}

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
    <!-- Compact Split Card Container -->
    <div
      class="w-full max-w-4xl bg-white rounded-3xl shadow-xl border border-slate-200/70 overflow-hidden flex flex-col md:flex-row my-auto"
    >
      <!-- Left: Natural Photo -->
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
          <div class="flex items-center gap-2 mb-4">
            <div class="w-8 h-8 rounded-xl bg-[#FF7A59] flex items-center justify-center text-white shadow-xs">
              <PawPrint class="w-4 h-4" />
            </div>
            <span class="text-lg font-bold text-slate-800 tracking-tight">Cat Sitter</span>
          </div>

          <!-- Title -->
          <div class="mb-3.5">
            <h1 class="text-xl font-bold text-slate-900 tracking-tight">เข้าสู่ระบบ</h1>
            <p class="text-xs text-slate-500 mt-0.5">ยินดีต้อนรับกลับมา กรุณากรอกอีเมลและรหัสผ่านเพื่อเข้าใช้งาน</p>
          </div>

          <!-- Error Alert -->
          <div
            v-if="errorMessage"
            class="mb-3 p-2.5 rounded-xl bg-red-50 border border-red-200 text-xs text-red-600 flex items-start gap-2"
          >
            <AlertCircle class="w-3.5 h-3.5 text-red-500 shrink-0 mt-0.5" />
            <span>{{ errorMessage }}</span>
          </div>

          <!-- Form Inputs -->
          <form class="space-y-2.5" @submit.prevent="handleSubmit">
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
              class="w-full py-2.5 px-4 rounded-xl bg-[#FF7A59] hover:bg-[#e86645] text-white font-medium text-sm transition disabled:opacity-50 cursor-pointer flex items-center justify-center gap-2 shadow-xs"
            >
              <Loader2 v-if="isSubmitting" class="w-3.5 h-3.5 animate-spin" />
              <span>{{ isSubmitting ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ' }}</span>
            </button>
          </form>

          <!-- Divider -->
          <div class="relative flex items-center justify-center my-3">
            <div class="border-t border-slate-200 w-full"></div>
            <span class="bg-white px-2.5 text-[11px] text-slate-400 font-medium">หรือเข้าสู่ระบบด้วย</span>
          </div>

          <!-- SSO Buttons: LINE & Google -->
          <div class="grid grid-cols-2 gap-2.5">
            <!-- LINE SSO Button -->
            <button
              type="button"
              id="btn-line-sso"
              @click="openLineModal"
              class="w-full py-2 px-3 rounded-xl bg-[#06C755] hover:bg-[#05b34c] text-white font-medium text-xs sm:text-sm transition cursor-pointer flex items-center justify-center gap-2 shadow-xs"
            >
              <svg class="w-4 h-4 fill-current shrink-0" viewBox="0 0 24 24">
                <path d="M19.365 9.863c.349 0 .63.285.63.631 0 .345-.281.63-.63.63H17.61v1.125h1.755c.349 0 .63.283.63.63 0 .344-.281.629-.63.629h-2.386c-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.63-.63h2.386c.346 0 .627.285.627.63 0 .349-.281.63-.63.63H17.61v1.125h1.755zm-3.855 3.016c0 .27-.174.51-.432.596-.064.021-.133.031-.199.031-.211 0-.391-.09-.51-.25l-2.443-3.317v2.94c0 .344-.279.629-.631.629-.346 0-.626-.285-.626-.629V8.108c0-.27.173-.51.43-.595.06-.023.136-.033.194-.033.195 0 .375.104.495.254l2.462 3.33V8.108c0-.345.282-.63.63-.63.345 0 .63.285.63.63v4.771zm-5.741 0c0 .344-.282.629-.631.629-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.63-.63.346 0 .628.285.628.63v4.771zm-2.466.629H4.917c-.345 0-.63-.285-.63-.629V8.108c0-.345.285-.63.63-.63.348 0 .63.285.63.63v4.141h1.756c.348 0 .629.283.629.63 0 .344-.282.629-.629.629M24 10.314C24 4.943 18.615.572 12 .572S0 4.943 0 10.314c0 4.811 4.27 8.842 10.035 9.608.391.082.923.258 1.058.59.12.301.079.766.038 1.08l-.164 1.02c-.045.301-.24 1.186 1.049.645 1.291-.539 6.916-4.078 9.436-6.975C23.176 13.393 24 11.91 24 10.314"/>
              </svg>
              <span>LINE</span>
            </button>

            <!-- Google SSO Button -->
            <button
              type="button"
              id="btn-google-sso"
              @click="openGoogleModal"
              class="w-full py-2 px-3 rounded-xl bg-white hover:bg-slate-50 border border-slate-200 text-slate-700 font-medium text-xs sm:text-sm transition cursor-pointer flex items-center justify-center gap-2 shadow-2xs"
            >
              <svg class="w-4 h-4 shrink-0" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"/>
              </svg>
              <span>Google</span>
            </button>
          </div>

          <!-- Subtle Demo accounts fill -->
          <div class="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between text-[11px] text-slate-500">
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
        <div class="mt-4 pt-3 border-t border-slate-100 text-center text-xs text-slate-500">
          <span>ยังไม่มีบัญชี? </span>
          <RouterLink to="/signup/owner" class="text-[#FF7A59] hover:underline font-medium">สมัครเป็นเจ้าของแมว</RouterLink>
          <span class="mx-1.5 text-slate-300">·</span>
          <RouterLink to="/signup/sitter" class="text-[#FF7A59] hover:underline font-medium">สมัครเป็นผู้รับฝากเลี้ยง</RouterLink>
        </div>
      </div>
    </div>

    <!-- Modal: LINE SSO OAuth Consent Screen -->
    <div
      v-if="showLineModal"
      class="fixed inset-0 bg-slate-950/60 backdrop-blur-xs z-50 flex items-center justify-center p-4 overflow-y-auto"
      @click.self="showLineModal = false"
    >
      <div class="bg-white rounded-2xl max-w-sm w-full shadow-2xl border border-slate-100 overflow-hidden my-6">
        <!-- LINE Modal Header -->
        <div class="px-5 py-4 bg-[#06C755] text-white flex items-center justify-between">
          <div class="flex items-center gap-2">
            <svg class="w-6 h-6 fill-current" viewBox="0 0 24 24">
              <path d="M19.365 9.863c.349 0 .63.285.63.631 0 .345-.281.63-.63.63H17.61v1.125h1.755c.349 0 .63.283.63.63 0 .344-.281.629-.63.629h-2.386c-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.63-.63h2.386c.346 0 .627.285.627.63 0 .349-.281.63-.63.63H17.61v1.125h1.755zm-3.855 3.016c0 .27-.174.51-.432.596-.064.021-.133.031-.199.031-.211 0-.391-.09-.51-.25l-2.443-3.317v2.94c0 .344-.279.629-.631.629-.346 0-.626-.285-.626-.629V8.108c0-.27.173-.51.43-.595.06-.023.136-.033.194-.033.195 0 .375.104.495.254l2.462 3.33V8.108c0-.345.282-.63.63-.63.345 0 .63.285.63.63v4.771zm-5.741 0c0 .344-.282.629-.631.629-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.63-.63.346 0 .628.285.628.63v4.771zm-2.466.629H4.917c-.345 0-.63-.285-.63-.629V8.108c0-.345.285-.63.63-.63.348 0 .63.285.63.63v4.141h1.756c.348 0 .629.283.629.63 0 .344-.282.629-.629.629M24 10.314C24 4.943 18.615.572 12 .572S0 4.943 0 10.314c0 4.811 4.27 8.842 10.035 9.608.391.082.923.258 1.058.59.12.301.079.766.038 1.08l-.164 1.02c-.045.301-.24 1.186 1.049.645 1.291-.539 6.916-4.078 9.436-6.975C23.176 13.393 24 11.91 24 10.314"/>
            </svg>
            <h3 class="font-bold text-base">LINE Login</h3>
          </div>
          <button
            type="button"
            @click="showLineModal = false"
            class="text-white/80 hover:text-white p-1 rounded-lg hover:bg-black/10 transition cursor-pointer"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <div class="p-5 space-y-4 text-xs">
          <div class="text-center space-y-1">
            <p class="font-bold text-sm text-slate-800">Cat Sitter Thailand</p>
            <p class="text-slate-500">ขอสิทธิ์เข้าถึงข้อมูลโปรไฟล์ LINE ของคุณ</p>
          </div>

          <!-- Profile Preview Box -->
          <div class="p-3.5 rounded-xl border border-slate-200 bg-slate-50/70 flex items-center gap-3">
            <img
              src="https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=256&q=80"
              alt="LINE Avatar"
              class="w-12 h-12 rounded-full object-cover border border-slate-200"
            />
            <div class="flex-1 min-w-0">
              <label class="block text-[11px] font-semibold text-slate-600 mb-0.5">ชื่อแสดงใน LINE</label>
              <input
                v-model="lineUserName"
                type="text"
                class="w-full px-2 py-1 rounded border border-slate-200 bg-white text-xs text-slate-800 font-medium focus:outline-none focus:border-[#06C755]"
              />
              <p class="text-[10px] text-slate-400 mt-0.5">อีเมล: line.member@line.me</p>
            </div>
          </div>

          <!-- Role selection if first-time user -->
          <div class="space-y-1.5">
            <label class="block font-semibold text-slate-700 text-xs">ประเภทบัญชีที่ต้องการเข้าใช้งาน:</label>
            <div class="grid grid-cols-2 gap-2">
              <label
                class="p-2.5 rounded-xl border cursor-pointer flex items-center gap-2 transition"
                :class="lineUserRole === 'owner' ? 'border-[#06C755] bg-emerald-50/50 text-emerald-950 font-semibold' : 'border-slate-200 bg-white text-slate-600'"
              >
                <input
                  v-model="lineUserRole"
                  type="radio"
                  value="owner"
                  class="text-[#06C755] focus:ring-[#06C755]"
                />
                <span>เจ้าของแมว</span>
              </label>

              <label
                class="p-2.5 rounded-xl border cursor-pointer flex items-center gap-2 transition"
                :class="lineUserRole === 'sitter' ? 'border-[#06C755] bg-emerald-50/50 text-emerald-950 font-semibold' : 'border-slate-200 bg-white text-slate-600'"
              >
                <input
                  v-model="lineUserRole"
                  type="radio"
                  value="sitter"
                  class="text-[#06C755] focus:ring-[#06C755]"
                />
                <span>ผู้รับฝากเลี้ยง</span>
              </label>
            </div>
          </div>

          <div class="p-2.5 bg-slate-50 border border-slate-200 rounded-lg text-[11px] text-slate-500 leading-relaxed">
            ระบบจะเชื่อมต่อบัญชีกับ LINE ID ของคุณ เพื่อให้เข้าสู่ระบบได้ทันทีในคลิกเดียว
          </div>
        </div>

        <div class="px-5 py-3.5 bg-slate-50 border-t border-slate-100 flex items-center justify-end gap-2.5">
          <button
            type="button"
            @click="showLineModal = false"
            class="px-3.5 py-2 text-xs font-medium text-slate-600 hover:bg-slate-200 rounded-lg transition cursor-pointer"
          >
            ยกเลิก
          </button>
          <button
            type="button"
            id="btn-confirm-line-sso"
            :disabled="isSubmittingSso"
            @click="handleLineSsoLogin"
            class="px-4 py-2 text-xs font-bold text-white bg-[#06C755] hover:bg-[#05b34c] rounded-lg shadow-xs disabled:opacity-50 transition cursor-pointer flex items-center gap-1.5"
          >
            <Loader2 v-if="isSubmittingSso" class="w-3.5 h-3.5 animate-spin" />
            <span>{{ isSubmittingSso ? 'กำลังเข้าสู่ระบบ...' : 'อนุญาตและเข้าสู่ระบบ' }}</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Modal: Google SSO Consent Screen -->
    <div
      v-if="showGoogleModal"
      class="fixed inset-0 bg-slate-950/60 z-50 flex items-center justify-center p-4 overflow-y-auto"
      @click.self="showGoogleModal = false"
    >
      <div class="bg-white rounded-2xl max-w-sm w-full shadow-2xl border border-slate-200 overflow-hidden my-6">
        <!-- Google Modal Header -->
        <div class="px-5 py-4 bg-white border-b border-slate-100 flex items-center justify-between">
          <div class="flex items-center gap-2.5">
            <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24">
              <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
              <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
              <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"/>
              <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"/>
            </svg>
            <h3 class="font-bold text-sm text-slate-800">ลงชื่อเข้าใช้ด้วย Google</h3>
          </div>
          <button
            type="button"
            @click="showGoogleModal = false"
            class="text-slate-400 hover:text-slate-600 p-1 rounded-lg hover:bg-slate-100 transition cursor-pointer"
          >
            <X class="w-5 h-5" />
          </button>
        </div>

        <div class="p-5 space-y-4 text-xs">
          <div class="text-center space-y-1">
            <p class="font-bold text-sm text-slate-800">Cat Sitter Thailand</p>
            <p class="text-slate-500">เลือกบัญชี Google เพื่อเข้าสู่ระบบ</p>
          </div>

          <!-- Account Preview Card -->
          <div class="p-3.5 rounded-xl border border-slate-200 bg-slate-50 flex items-center gap-3">
            <div class="w-11 h-11 rounded-full bg-[#4285F4] text-white flex items-center justify-center font-bold text-base shrink-0 shadow-xs">
              {{ googleUserName.charAt(0) || 'G' }}
            </div>
            <div class="flex-1 min-w-0">
              <label class="block text-[11px] font-semibold text-slate-600 mb-0.5">ชื่อบัญชี Google</label>
              <input
                v-model="googleUserName"
                type="text"
                class="w-full px-2 py-1 rounded border border-slate-200 bg-white text-xs text-slate-800 font-medium focus:outline-none focus:border-[#4285F4]"
              />
              <p class="text-[10px] text-slate-400 mt-0.5">{{ googleUserEmail }}</p>
            </div>
          </div>

          <!-- Role selection -->
          <div class="space-y-1.5">
            <label class="block font-semibold text-slate-700 text-xs">ประเภทบัญชีที่ต้องการเข้าใช้งาน:</label>
            <div class="grid grid-cols-2 gap-2">
              <label
                class="p-2.5 rounded-xl border cursor-pointer flex items-center gap-2 transition"
                :class="googleUserRole === 'owner' ? 'border-[#4285F4] bg-blue-50/50 text-blue-950 font-semibold' : 'border-slate-200 bg-white text-slate-600'"
              >
                <input
                  v-model="googleUserRole"
                  type="radio"
                  value="owner"
                  class="text-[#4285F4] focus:ring-[#4285F4]"
                />
                <span>เจ้าของแมว</span>
              </label>

              <label
                class="p-2.5 rounded-xl border cursor-pointer flex items-center gap-2 transition"
                :class="googleUserRole === 'sitter' ? 'border-[#4285F4] bg-blue-50/50 text-blue-950 font-semibold' : 'border-slate-200 bg-white text-slate-600'"
              >
                <input
                  v-model="googleUserRole"
                  type="radio"
                  value="sitter"
                  class="text-[#4285F4] focus:ring-[#4285F4]"
                />
                <span>ผู้รับฝากเลี้ยง</span>
              </label>
            </div>
          </div>

          <div class="p-2.5 bg-slate-50 border border-slate-200 rounded-lg text-[11px] text-slate-500 leading-relaxed">
            ระบบจะเชื่อมต่อบัญชี Google ของคุณเพื่อเข้าใช้งานได้อย่างสะดวกรวดเร็วและปลอดภัย
          </div>
        </div>

        <div class="px-5 py-3.5 bg-slate-50 border-t border-slate-100 flex items-center justify-end gap-2.5">
          <button
            type="button"
            @click="showGoogleModal = false"
            class="px-3.5 py-2 text-xs font-medium text-slate-600 hover:bg-slate-200 rounded-lg transition cursor-pointer"
          >
            ยกเลิก
          </button>
          <button
            type="button"
            id="btn-confirm-google-sso"
            :disabled="isSubmittingSso"
            @click="handleGoogleSsoLogin"
            class="px-4 py-2 text-xs font-bold text-white bg-[#1a73e8] hover:bg-[#1557b0] rounded-lg shadow-xs disabled:opacity-50 transition cursor-pointer flex items-center gap-1.5"
          >
            <Loader2 v-if="isSubmittingSso" class="w-3.5 h-3.5 animate-spin" />
            <span>{{ isSubmittingSso ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบด้วย Google' }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
