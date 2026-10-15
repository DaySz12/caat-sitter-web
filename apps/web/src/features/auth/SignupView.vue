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
  Loader2
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

const isSitter = computed(() => props.role === 'sitter');
const title = computed(() => (isSitter.value ? 'สมัครเป็นผู้รับฝากเลี้ยง' : 'สมัครเป็นเจ้าของแมว'));

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
          <div class="flex items-center justify-between mb-4">
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
          <div class="mb-3.5">
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
          <form class="space-y-2.5" @submit.prevent="handleSubmit">
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
                  class="w-full pl-8 pr-3 py-1.5 rounded-lg border border-slate-200 text-xs text-slate-900 placeholder:text-slate-400 focus:outline-none focus:border-[#FF7A59] focus:ring-1 focus:ring-[#FF7A59] transition"
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
        </div>

        <div class="mt-4 pt-3 border-t border-slate-100 text-center text-xs text-slate-500">
          <span>มีบัญชีอยู่แล้ว? </span>
          <RouterLink to="/login" class="text-[#FF7A59] hover:underline font-medium">เข้าสู่ระบบที่นี่</RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>
