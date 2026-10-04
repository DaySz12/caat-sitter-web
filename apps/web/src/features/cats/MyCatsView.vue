<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { Plus, Cat as CatIcon } from 'lucide-vue-next';
import { catsApi } from './cats.api';
import CatCard from './CatCard.vue';
import type { CatDto } from '@/shared/types/models';

const router = useRouter();
const cats = ref<CatDto[]>([]);
const isLoading = ref(true);

async function load() {
  isLoading.value = true;
  cats.value = await catsApi.mine();
  isLoading.value = false;
}

onMounted(load);

function handleEdit(cat: CatDto) {
  router.push(`/owner/cats/${cat.id}/edit`);
}

async function handleRemove(cat: CatDto) {
  if (!confirm(`ต้องการลบข้อมูลของ ${cat.name} ใช่หรือไม่?`)) return;
  await catsApi.remove(cat.id);
  await load();
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between mb-5">
      <div>
        <h1 class="text-xl font-bold text-slate-800">แมวของฉัน</h1>
        <p class="text-sm text-slate-500">จัดการข้อมูลแมวที่จะฝากดูแล</p>
      </div>
      <button
        class="flex items-center gap-1.5 px-4 py-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white text-sm font-semibold"
        @click="router.push('/owner/cats/new')"
      >
        <Plus class="w-4 h-4" /> เพิ่มแมว
      </button>
    </div>

    <div v-if="isLoading" class="text-center py-16 text-slate-400 text-sm">กำลังโหลด...</div>

    <div v-else-if="cats.length === 0" class="text-center py-16 bg-white rounded-2xl border border-dashed border-slate-200">
      <CatIcon class="w-10 h-10 text-slate-300 mx-auto mb-3" />
      <p class="text-slate-500 text-sm">ยังไม่มีข้อมูลแมว เริ่มเพิ่มแมวตัวแรกของคุณกันเลย</p>
    </div>

    <div v-else class="grid sm:grid-cols-2 gap-4">
      <CatCard v-for="cat in cats" :key="cat.id" :cat="cat" @edit="handleEdit" @remove="handleRemove" />
    </div>
  </div>
</template>
