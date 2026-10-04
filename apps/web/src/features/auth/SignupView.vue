<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { useAuthStore } from './auth.store';
import { PawPrint, User, Mail, Lock, Phone, AlertCircle, Loader2 } from 'lucide-vue-next';

const props = defineProps<{ role: 'owner' | 'sitter' }>();

const auth = useAuthStore();
const router = useRouter();

const name = ref('');
const email = ref('');
const phone = ref('');
const password = ref('');
const confirmPassword = ref('');
const isSubmitting = ref(false);
const errorMessage = ref('');

const title = computed(() => (props.role === 'sitter' ? 'สมัครเป็นผู้รับฝากเลี้ยงแมว' : 'สมัครสมาชิก (เจ้าของแมว)'));

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
    errorMessage.value = 'รหัสผ่านไม่ตรงกัน';
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
  <div class="min-h-screen bg-[#1E2238] flex items-center justify-center p-4 relative overflow-hidden">
    <div class="absolute -top-40 -left-40 w-96 h-96 bg-[#FF7A59]/20 rounded-full blur-3xl pointer-events-none"></div>
    <div class="absolute -bottom-40 -right-40 w-96 h-96 bg-[#FFC857]/15 rounded-full blur-3xl pointer-events-none"></div>

    <div class="w-full max-w-md bg-white rounded-3xl p-8 shadow-2xl relative z-10 border border-slate-100">
      <div class="text-center mb-6">
        <div class="w-16 h-16 rounded-2xl bg-gradient-to-tr from-[#FF7A59] to-[#FF9E85] mx-auto flex items-center justify-center shadow-xl shadow-[#FF7A59]/30 mb-4">
          <PawPrint class="w-9 h-9 text-white" />
        </div>
        <h1 class="text-xl font-bold text-slate-800">{{ title }}</h1>
        <p v-if="role === 'sitter'" class="text-xs text-slate-500 mt-1">
          บัญชีของคุณจะต้องรอการตรวจสอบจากผู้ดูแลระบบก่อนเริ่มรับงานได้
        </p>
      </div>

      <div v-if="errorMessage" class="mb-5 p-4 rounded-xl bg-red-50 border border-red-200 text-sm text-red-600 flex items-start gap-3">
        <AlertCircle class="w-5 h-5 shrink-0 text-red-500 mt-0.5" />
        <span>{{ errorMessage }}</span>
      </div>

      <form class="space-y-3.5" @submit.prevent="handleSubmit">
        <div class="relative">
          <User class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input v-model="name" type="text" placeholder="ชื่อ-นามสกุล"
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>
        <div class="relative">
          <Mail class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input v-model="email" type="email" placeholder="อีเมล"
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>
        <div class="relative">
          <Phone class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input v-model="phone" type="tel" placeholder="เบอร์โทรศัพท์ (ไม่บังคับ)"
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>
        <div class="relative">
          <Lock class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input v-model="password" type="password" placeholder="รหัสผ่าน (อย่างน้อย 6 ตัวอักษร)"
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>
        <div class="relative">
          <Lock class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input v-model="confirmPassword" type="password" placeholder="ยืนยันรหัสผ่าน"
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:ring-2 focus:ring-[#FF7A59]/30 focus:border-[#FF7A59]" />
        </div>

        <button type="submit" :disabled="isSubmitting"
          class="w-full py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm flex items-center justify-center gap-2 transition disabled:opacity-60">
          <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
          สมัครสมาชิก
        </button>
      </form>

      <p class="mt-6 text-center text-sm text-slate-500">
        มีบัญชีอยู่แล้ว?
        <RouterLink to="/login" class="text-[#FF7A59] font-semibold hover:underline">เข้าสู่ระบบ</RouterLink>
      </p>
    </div>
  </div>
</template>
