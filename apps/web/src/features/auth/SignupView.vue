<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useAuthStore } from './auth.store';
import {
  PawPrint,
  User,
  Mail,
  Lock,
  Phone,
  Eye,
  EyeOff,
  AlertCircle,
  Loader2,
  X
} from 'lucide-vue-next';

const props = defineProps<{ role: 'owner' | 'sitter' }>();

const auth = useAuthStore();
const router = useRouter();

const name = ref('');
const email = ref('');
const phone = ref('');
const password = ref('');
const confirmPassword = ref('');
const showPassword = ref(false);
const isSubmitting = ref(false);
const errorMessage = ref('');

// LINE SSO Modal state
const showLineModal = ref(false);
const lineUserName = ref('คุณเหมียว (LINE Member)');
const isSubmittingSso = ref(false);

// Google SSO Modal state
const showGoogleModal = ref(false);
const googleUserName = ref('สมชาย ใจดี (Google)');
const googleUserEmail = ref('somchai.google@gmail.com');

function openGoogleModal() {
  errorMessage.value = '';
  showGoogleModal.value = true;
}

async function handleGoogleSsoSignup() {
  isSubmittingSso.value = true;
  errorMessage.value = '';

  const success = await auth.ssoLogin({
    provider: 'google',
    ssoId: `GOOGLE_${Date.now()}`,
    name: googleUserName.value.trim() || 'Google User',
    email: googleUserEmail.value.trim() || `google.${Date.now()}@gmail.com`,
    role: props.role,
    avatarUrl: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?auto=format&fit=crop&w=256&q=80',
  });

  isSubmittingSso.value = false;

  if (success) {
    showGoogleModal.value = false;
    router.push(props.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'สมัครสมาชิกด้วย Google ไม่สำเร็จ';
    showGoogleModal.value = false;
  }
}

const isSitter = computed(() => props.role === 'sitter');
const title = computed(() => (isSitter.value ? 'สมัครเป็นผู้รับฝากเลี้ยง' : 'สมัครเป็นเจ้าของแมว'));

function openLineModal() {
  errorMessage.value = '';
  showLineModal.value = true;
}

async function handleLineSsoSignup() {
  isSubmittingSso.value = true;
  errorMessage.value = '';

  const success = await auth.ssoLogin({
    provider: 'line',
    ssoId: `LINE_${Date.now()}`,
    name: lineUserName.value.trim() || 'LINE User',
    email: `line.${Date.now()}@line.me`,
    role: props.role,
    avatarUrl: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=256&q=80',
  });

  isSubmittingSso.value = false;

  if (success) {
    showLineModal.value = false;
    router.push(props.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'สมัครสมาชิกด้วย LINE ไม่สำเร็จ';
    showLineModal.value = false;
  }
}

async function handleSubmit() {
  errorMessage.value = '';
  if (!name.value || !email.value || !password.value) {
    errorMessage.value = 'กรุณากรอกข้อมูลให้ครบถ้วน';
    return;
  }
  if (password.value.length < 6) {
    errorMessage.value = 'รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร';
    return;
  }
  if (password.value !== confirmPassword.value) {
    errorMessage.value = 'รหัสผ่านไม่ตรงกัน กรุณาตรวจสอบอีกครั้ง';
    return;
  }

  isSubmitting.value = true;
  const success = await auth.register({
    email: email.value,
    password: password.value,
    name: name.value,
    phone: phone.value || undefined,
    role: props.role,
  });
  isSubmitting.value = false;

  if (success) {
    router.push(props.role === 'sitter' ? '/sitter' : '/owner');
  } else {
    errorMessage.value = auth.error || 'สมัครสมาชิกไม่สำเร็จ';
  }
}
</script>

<template>
  <div class="min-h-screen bg-slate-100/70 flex items-center justify-center p-4 sm:p-6">
    <!-- Compact Split Card Container -->
    <div
      class="w-full max-w-4xl bg-white rounded-3xl shadow-xl border border-slate-200/70 overflow-hidden flex flex-col md:flex-row my-auto"
    >
      <!-- Left: Natural Photo -->
      <div class="hidden md:block md:w-1/2 relative bg-slate-100 select-none min-h-[540px]">
        <img
          src="/images/login-hero.jpg"
          alt="Cat Sitter"
          class="absolute inset-0 w-full h-full object-cover object-center"
        />
      </div>

      <!-- Right: Compact Clean Signup Form -->
      <div class="w-full md:w-1/2 p-6 sm:p-8 flex flex-col justify-between">
        <div>
          <!-- Top Header & Role Switcher -->
          <div class="flex items-center justify-between mb-3">
            <RouterLink to="/" class="flex items-center gap-2">
              <div class="w-7 h-7 rounded-xl bg-[#FF7A59] flex items-center justify-center text-white">
                <PawPrint class="w-4 h-4" />
              </div>
              <span class="text-base font-bold text-slate-800 tracking-tight">Cat Sitter</span>
            </RouterLink>

            <div class="inline-flex p-0.5 rounded-lg bg-slate-100 text-xs font-medium">
              <RouterLink
                to="/signup/owner"
                class="px-2.5 py-1 rounded-md transition text-[11px]"
                :class="!isSitter ? 'bg-white text-slate-900 shadow-2xs font-semibold' : 'text-slate-500 hover:text-slate-900'"
              >
                เจ้าของแมว
              </RouterLink>
              <RouterLink
                to="/signup/sitter"
                class="px-2.5 py-1 rounded-md transition text-[11px]"
                :class="isSitter ? 'bg-white text-slate-900 shadow-2xs font-semibold' : 'text-slate-500 hover:text-slate-900'"
              >
                ผู้รับฝากเลี้ยง
              </RouterLink>
            </div>
          </div>

          <!-- Title -->
          <div class="mb-3">
            <h1 class="text-lg font-bold text-slate-900 tracking-tight">{{ title }}</h1>
            <p class="text-xs text-slate-500 mt-0.5">
              {{ isSitter ? 'สร้างบัญชีเพื่อเริ่มต้นรับงานดูแลน้องแมว' : 'สร้างบัญชีเพื่อจองพี่เลี้ยงดูแลน้องแมว' }}
            </p>
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
          <form class="space-y-2" @submit.prevent="handleSubmit">
            <div>
              <label class="block text-[11px] font-medium text-slate-700 mb-0.5">ชื่อ-นามสกุล <span class="text-red-500">*</span></label>
              <div class="relative">
                <User class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="name"
                  type="text"
                  required
                  placeholder="สมชาย ใจดี"
                  class="w-full pl-8 pr-3 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-medium text-slate-700 mb-0.5">อีเมล <span class="text-red-500">*</span></label>
              <div class="relative">
                <Mail class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="email"
                  type="email"
                  required
                  autocomplete="email"
                  placeholder="you@example.com"
                  class="w-full pl-8 pr-3 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-medium text-slate-700 mb-0.5">เบอร์โทรศัพท์ (ไม่บังคับ)</label>
              <div class="relative">
                <Phone class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="phone"
                  type="tel"
                  placeholder="081-234-5678"
                  class="w-full pl-8 pr-3 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-medium text-slate-700 mb-0.5">รหัสผ่าน (อย่างน้อย 6 ตัวอักษร) <span class="text-red-500">*</span></label>
              <div class="relative">
                <Lock class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="password"
                  :type="showPassword ? 'text' : 'password'"
                  required
                  autocomplete="new-password"
                  placeholder="••••••••"
                  class="w-full pl-8 pr-8 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
                <button
                  type="button"
                  @click="showPassword = !showPassword"
                  class="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 p-1 cursor-pointer"
                  tabindex="-1"
                >
                  <EyeOff v-if="showPassword" class="w-3 h-3" />
                  <Eye v-else class="w-3 h-3" />
                </button>
              </div>
            </div>

            <div>
              <label class="block text-[11px] font-medium text-slate-700 mb-0.5">ยืนยันรหัสผ่าน <span class="text-red-500">*</span></label>
              <div class="relative">
                <Lock class="w-3.5 h-3.5 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  v-model="confirmPassword"
                  :type="showPassword ? 'text' : 'password'"
                  required
                  autocomplete="new-password"
                  placeholder="••••••••"
                  class="w-full pl-8 pr-4 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
                />
              </div>
            </div>

            <button
              type="submit"
              :disabled="isSubmitting"
              class="w-full py-2 px-4 rounded-xl bg-[#FF7A59] hover:bg-[#e86645] text-white font-medium text-xs transition disabled:opacity-50 cursor-pointer flex items-center justify-center gap-1.5 mt-2"
            >
              <Loader2 v-if="isSubmitting" class="w-3.5 h-3.5 animate-spin" />
              <span>{{ isSubmitting ? 'กำลังสร้างบัญชี...' : 'สมัครสมาชิก' }}</span>
            </button>
          </form>

          <!-- Divider -->
          <div class="relative flex items-center justify-center my-2.5">
            <div class="border-t border-slate-200 w-full"></div>
            <span class="bg-white px-2 text-[10px] text-slate-400 font-medium">หรือสมัครด่วนด้วย</span>
          </div>

          <!-- Quick SSO Buttons: LINE & Google -->
          <div class="grid grid-cols-2 gap-2 mb-1">
            <!-- LINE SSO -->
            <button
              type="button"
              id="btn-line-signup-sso"
              @click="openLineModal"
              class="w-full py-1.5 px-3 rounded-lg bg-[#06C755] hover:bg-[#05b34c] text-white font-medium text-xs transition cursor-pointer flex items-center justify-center gap-1.5 shadow-xs"
            >
              <svg class="w-3.5 h-3.5 fill-current shrink-0" viewBox="0 0 24 24">
                <path d="M19.365 9.863c.349 0 .63.285.63.631 0 .345-.281.63-.63.63H17.61v1.125h1.755c.349 0 .63.283.63.63 0 .344-.281.629-.63.629h-2.386c-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.63-.63h2.386c.346 0 .627.285.627.63 0 .349-.281.63-.63.63H17.61v1.125h1.755zm-3.855 3.016c0 .27-.174.51-.432.596-.064.021-.133.031-.199.031-.211 0-.391-.09-.51-.25l-2.443-3.317v2.94c0 .344-.279.629-.631.629-.346 0-.626-.285-.626-.629V8.108c0-.27.173-.51.43-.595.06-.023.136-.033.194-.033.195 0 .375.104.495.254l2.462 3.33V8.108c0-.345.282-.63.63-.63.345 0 .63.285.63.63v4.771zm-5.741 0c0 .344-.282.629-.631.629-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.63-.63.346 0 .628.285.628.63v4.771zm-2.466.629H4.917c-.345 0-.63-.285-.63-.629V8.108c0-.345.285-.63.63-.63.348 0 .63.285.63.63v4.141h1.756c.348 0 .629.283.629.63 0 .344-.282.629-.629.629M24 10.314C24 4.943 18.615.572 12 .572S0 4.943 0 10.314c0 4.811 4.27 8.842 10.035 9.608.391.082.923.258 1.058.59.12.301.079.766.038 1.08l-.164 1.02c-.045.301-.24 1.186 1.049.645 1.291-.539 6.916-4.078 9.436-6.975C23.176 13.393 24 11.91 24 10.314"/>
              </svg>
              <span>LINE</span>
            </button>

            <!-- Google SSO -->
            <button
              type="button"
              id="btn-google-signup-sso"
              @click="openGoogleModal"
              class="w-full py-1.5 px-3 rounded-lg bg-white hover:bg-slate-50 border border-slate-200 text-slate-700 font-medium text-xs transition cursor-pointer flex items-center justify-center gap-1.5 shadow-2xs"
            >
              <svg class="w-3.5 h-3.5 shrink-0" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z"/>
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z"/>
              </svg>
              <span>Google</span>
            </button>
          </div>
        </div>

        <div class="mt-3 pt-2.5 border-t border-slate-100 text-center text-xs text-slate-500">
          <span>มีบัญชีอยู่แล้ว? </span>
          <RouterLink to="/login" class="text-[#FF7A59] hover:underline font-medium">เข้าสู่ระบบที่นี่</RouterLink>
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
            <p class="text-slate-500">สมัครสมาชิกด้วยบัญชี LINE</p>
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
              <p class="text-[10px] text-slate-400 mt-0.5">สมัครในบทบาท: <strong class="text-slate-700">{{ isSitter ? 'ผู้รับฝากเลี้ยง' : 'เจ้าของแมว' }}</strong></p>
            </div>
          </div>

          <div class="p-2.5 bg-slate-50 border border-slate-200 rounded-lg text-[11px] text-slate-500 leading-relaxed">
            ระบบจะสร้างบัญชีและเชื่อมต่อกับ LINE ID ของคุณทันที เพื่อความสะดวกรวดเร็ว
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
            id="btn-confirm-line-signup"
            :disabled="isSubmittingSso"
            @click="handleLineSsoSignup"
            class="px-4 py-2 text-xs font-bold text-white bg-[#06C755] hover:bg-[#05b34c] rounded-lg shadow-xs disabled:opacity-50 transition cursor-pointer flex items-center gap-1.5"
          >
            <Loader2 v-if="isSubmittingSso" class="w-3.5 h-3.5 animate-spin" />
            <span>{{ isSubmittingSso ? 'กำลังสมัคร...' : 'ยืนยันสมัครด้วย LINE' }}</span>
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
            <h3 class="font-bold text-sm text-slate-800">สมัครด้วย Google</h3>
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
            <p class="text-slate-500">สมัครสมาชิกด้วยบัญชี Google</p>
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
              <p class="text-[10px] text-slate-400 mt-0.5">สมัครในบทบาท: <strong class="text-slate-700">{{ isSitter ? 'ผู้รับฝากเลี้ยง' : 'เจ้าของแมว' }}</strong></p>
            </div>
          </div>

          <div class="p-2.5 bg-slate-50 border border-slate-200 rounded-lg text-[11px] text-slate-500 leading-relaxed">
            ระบบจะสร้างบัญชี Cat Sitter และเชื่อมต่อกับ Google ID ของคุณทันที
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
            id="btn-confirm-google-signup"
            :disabled="isSubmittingSso"
            @click="handleGoogleSsoSignup"
            class="px-4 py-2 text-xs font-bold text-white bg-[#1a73e8] hover:bg-[#1557b0] rounded-lg shadow-xs disabled:opacity-50 transition cursor-pointer flex items-center gap-1.5"
          >
            <Loader2 v-if="isSubmittingSso" class="w-3.5 h-3.5 animate-spin" />
            <span>{{ isSubmittingSso ? 'กำลังสมัคร...' : 'ยืนยันสมัครด้วย Google' }}</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
