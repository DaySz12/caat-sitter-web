<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Loader2, AlertCircle } from 'lucide-vue-next';
import { sittersApi } from '@/features/sitters/sitters.api';
import { catsApi } from '@/features/cats/cats.api';
import { bookingApi } from './booking.api';
import type { SitterDto, CatDto } from '@/shared/types/models';
import { formatCurrency, formatPerDay } from '@/shared/lib/format';

const route = useRoute();
const router = useRouter();

const sitter = ref<SitterDto | null>(null);
const cats = ref<CatDto[]>([]);
const selectedCatIds = ref<string[]>([]);
const startDate = ref('');
const endDate = ref('');
const specialInstructions = ref('');
const isLoading = ref(true);
const isSubmitting = ref(false);
const errorMessage = ref('');

const daysCount = computed(() => {
  if (!startDate.value || !endDate.value) return 0;
  const start = new Date(startDate.value);
  const end = new Date(endDate.value);
  const diff = Math.round((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
  return diff >= 0 ? diff + 1 : 0;
});

const basePrice = computed(() => {
  if (!sitter.value || selectedCatIds.value.length === 0 || daysCount.value === 0) return 0;
  return sitter.value.pricePerDay * daysCount.value * selectedCatIds.value.length;
});
const serviceFee = computed(() => Math.round(basePrice.value * 0.1 * 100) / 100);
const totalPrice = computed(() => basePrice.value + serviceFee.value);

function toggleCat(id: string) {
  const idx = selectedCatIds.value.indexOf(id);
  if (idx >= 0) selectedCatIds.value.splice(idx, 1);
  else selectedCatIds.value.push(id);
}

onMounted(async () => {
  const sitterId = route.query.sitterId as string;
  if (!sitterId) {
    router.replace('/owner/sitters');
    return;
  }
  const [sitterData, catsData] = await Promise.all([sittersApi.getById(sitterId), catsApi.mine()]);
  sitter.value = sitterData;
  cats.value = catsData;
  isLoading.value = false;
});

async function handleSubmit() {
  errorMessage.value = '';
  if (selectedCatIds.value.length === 0) {
    errorMessage.value = 'กรุณาเลือกแมวอย่างน้อย 1 ตัว';
    return;
  }
  if (!startDate.value || !endDate.value) {
    errorMessage.value = 'กรุณาเลือกวันที่ฝากเลี้ยง';
    return;
  }
  if (!sitter.value) return;

  isSubmitting.value = true;
  try {
    const booking = await bookingApi.create({
      sitterId: sitter.value.id,
      catIds: selectedCatIds.value,
      startDate: startDate.value,
      endDate: endDate.value,
      specialInstructions: specialInstructions.value || undefined,
    });
    router.push(`/owner/bookings?highlight=${booking.id}`);
  } catch (err: any) {
    errorMessage.value = err.message || 'ส่งคำขอจองไม่สำเร็จ';
  } finally {
    isSubmitting.value = false;
  }
}
</script>

<template>
  <div class="max-w-lg mx-auto">
    <button class="flex items-center gap-1.5 text-sm text-slate-500 hover:text-slate-700 mb-4" @click="router.back()">
      <ArrowLeft class="w-4 h-4" /> กลับ
    </button>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>

    <div v-else-if="sitter" class="bg-white rounded-2xl border border-slate-100 shadow-sm p-6 space-y-5">
      <div>
        <h1 class="text-lg font-bold text-slate-800">จองบริการ: {{ sitter.user?.name }}</h1>
        <p class="text-sm text-slate-500">อัตราค่าบริการ: {{ formatPerDay(sitter.pricePerDay) }}</p>
      </div>

      <div v-if="errorMessage" class="p-3 rounded-xl bg-red-50 border border-red-200 text-sm text-red-600 flex items-start gap-2">
        <AlertCircle class="w-4 h-4 shrink-0 mt-0.5" /> {{ errorMessage }}
      </div>

      <div>
        <label class="block text-sm font-medium text-slate-700 mb-2">เลือกแมวที่จะฝากเลี้ยง</label>
        <div v-if="cats.length === 0" class="text-xs text-slate-400">
          ยังไม่มีข้อมูลแมว
          <RouterLink to="/owner/cats/new" class="text-[#FF7A59] font-semibold">เพิ่มแมว</RouterLink>
        </div>
        <div v-else class="space-y-2">
          <label
            v-for="cat in cats"
            :key="cat.id"
            class="flex items-center gap-2 p-2.5 rounded-xl border cursor-pointer text-sm"
            :class="selectedCatIds.includes(cat.id) ? 'border-[#FF7A59] bg-[#FFF0EB]' : 'border-slate-200'"
          >
            <input type="checkbox" :checked="selectedCatIds.includes(cat.id)" class="rounded text-[#FF7A59]" @change="toggleCat(cat.id)" />
            {{ cat.name }}
          </label>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-3">
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">วันที่เริ่ม</label>
          <input v-model="startDate" type="date" class="w-full px-3 py-2.5 rounded-xl border border-slate-200 text-sm" />
        </div>
        <div>
          <label class="block text-sm font-medium text-slate-700 mb-1.5">วันที่สิ้นสุด</label>
          <input v-model="endDate" type="date" class="w-full px-3 py-2.5 rounded-xl border border-slate-200 text-sm" />
        </div>
      </div>
      <p v-if="daysCount > 0" class="text-xs text-slate-500">รวม {{ daysCount }} วัน</p>

      <div>
        <label class="block text-sm font-medium text-slate-700 mb-1.5">คำแนะนำพิเศษ (ไม่บังคับ)</label>
        <textarea v-model="specialInstructions" rows="2" class="w-full px-4 py-2.5 rounded-xl border border-slate-200 text-sm"></textarea>
      </div>

      <div class="bg-slate-50 rounded-xl p-4 space-y-1.5 text-sm">
        <div class="flex justify-between text-slate-600">
          <span>ค่าบริการ ({{ selectedCatIds.length }} ตัว x {{ daysCount }} วัน)</span>
          <span class="font-medium">{{ formatCurrency(basePrice) }}</span>
        </div>
        <div class="flex justify-between text-slate-600">
          <span>ค่าธรรมเนียมแพลตฟอร์ม (10%)</span>
          <span class="font-medium">{{ formatCurrency(serviceFee) }}</span>
        </div>
        <div class="flex justify-between text-base font-bold text-[#FF7A59] pt-1.5 border-t border-slate-200">
          <span>รวมทั้งหมด</span>
          <span>{{ formatCurrency(totalPrice) }}</span>
        </div>
      </div>

      <button
        :disabled="isSubmitting"
        class="w-full py-3 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white font-semibold text-sm flex items-center justify-center gap-2 disabled:opacity-60"
        @click="handleSubmit"
      >
        <Loader2 v-if="isSubmitting" class="w-4 h-4 animate-spin" />
        ยืนยันและส่งคำขอจอง (฿{{ Math.round(totalPrice) }})
      </button>
    </div>
  </div>
</template>
