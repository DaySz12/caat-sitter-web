<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { api } from '../api/client';
import { Search, Ban, CheckCircle } from 'lucide-vue-next';

const users = ref<any[]>([]);
const isLoading = ref(true);
const searchQuery = ref('');
const roleFilter = ref('all');

async function loadUsers() {
  isLoading.value = true;
  try {
    users.value = await api.get<any[]>(`/api/admin/users?role=${roleFilter.value}`);
  } catch (e) {
    console.error('Error loading users:', e);
  } finally {
    isLoading.value = false;
  }
}

const filteredUsers = computed(() => {
  if (!searchQuery.value.trim()) return users.value;
  const q = searchQuery.value.toLowerCase();
  return users.value.filter(
    u => u.name?.toLowerCase().includes(q) || u.email?.toLowerCase().includes(q) || u.phone?.includes(q)
  );
});

async function toggleSuspend(user: any) {
  const newStatus = user.status === 'suspended' ? 'approved' : 'suspended';
  const confirmMsg = user.status === 'suspended' ? 'ต้องการยกเลิกการระงับบัญชีนี้ใช่หรือไม่?' : 'ต้องการระงับบัญชีนี้ชั่วคราวใช่หรือไม่?';
  if (!confirm(confirmMsg)) return;

  try {
    await api.patch(`/api/admin/users/${user.id}/status`, { status: newStatus });
    user.status = newStatus;
  } catch (e) {
    console.error('Error toggling suspend:', e);
  }
}

onMounted(() => {
  loadUsers();
});
</script>

<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">จัดการผู้ใช้งานทั้งหมด (Users Management)</h1>
        <p class="text-xs text-slate-500 mt-1">รายชื่อผู้ใช้ทั้งฝั่ง Owner, Sitter และ Admin พร้อมจัดการสิทธิ์</p>
      </div>

      <div class="flex flex-wrap items-center gap-3">
        <div class="relative">
          <Search class="w-4 h-4 text-slate-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="ค้นหาชื่อ หรืออีเมล..."
            class="pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 bg-white text-xs w-56 focus:outline-none focus:ring-2 focus:ring-[#FF7A59]"
          />
        </div>

        <select
          v-model="roleFilter"
          @change="loadUsers"
          class="px-3.5 py-2.5 rounded-xl border border-slate-200 bg-white text-xs text-slate-700 font-semibold focus:outline-none focus:ring-2 focus:ring-[#FF7A59] cursor-pointer"
        >
          <option value="all">ทุกบทบาท (All Roles)</option>
          <option value="owner">เจ้าของแมว (Owner)</option>
          <option value="sitter">ผู้รับเลี้ยง (Sitter)</option>
          <option value="admin">ผู้ดูแลระบบ (Admin)</option>
        </select>
      </div>
    </div>

    <!-- Table -->
    <div class="bg-white rounded-2xl border border-slate-200/80 shadow-xs overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-slate-600">
          <thead class="bg-slate-50 text-xs uppercase font-semibold text-slate-500 border-b border-slate-100">
            <tr>
              <th class="py-3.5 px-6">ผู้ใช้งาน</th>
              <th class="py-3.5 px-6">บทบาท</th>
              <th class="py-3.5 px-6">เบอร์โทรศัพท์</th>
              <th class="py-3.5 px-6">สถานะบัญชี</th>
              <th class="py-3.5 px-6">วันที่สมัคร</th>
              <th class="py-3.5 px-6 text-right">การจัดการ</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-100">
            <tr v-if="isLoading" class="text-center py-8">
              <td colspan="6" class="py-8 text-slate-400">กำลังโหลดรายชื่อผู้ใช้...</td>
            </tr>
            <tr v-else-if="filteredUsers.length === 0" class="text-center py-8">
              <td colspan="6" class="py-8 text-slate-400">ไม่พบรายชื่อผู้ใช้ที่ตรงกับเงื่อนไข</td>
            </tr>
            <tr v-for="u in filteredUsers" :key="u.id" class="hover:bg-slate-50/60 transition-colors">
              <td class="py-4 px-6">
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-full bg-slate-100 border border-slate-200 overflow-hidden shrink-0">
                    <img v-if="u.avatar_url" :src="u.avatar_url" class="w-full h-full object-cover" />
                    <div v-else class="w-full h-full flex items-center justify-center font-bold text-slate-500 text-xs">
                      {{ u.name?.charAt(0) || 'U' }}
                    </div>
                  </div>
                  <div>
                    <p class="font-semibold text-slate-800">{{ u.name }}</p>
                    <p class="text-xs text-slate-400">{{ u.email }}</p>
                  </div>
                </div>
              </td>
              <td class="py-4 px-6">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold',
                    u.role === 'admin' ? 'bg-purple-100 text-purple-800' :
                    u.role === 'sitter' ? 'bg-amber-100 text-amber-800' :
                    'bg-blue-100 text-blue-800'
                  ]"
                >
                  {{ u.role === 'admin' ? 'Admin' : u.role === 'sitter' ? 'Sitter' : 'Owner' }}
                </span>
              </td>
              <td class="py-4 px-6 text-xs text-slate-600">
                {{ u.phone || '-' }}
              </td>
              <td class="py-4 px-6">
                <span
                  :class="[
                    'inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold',
                    u.status === 'suspended' ? 'bg-rose-100 text-rose-800' :
                    u.status === 'pending' ? 'bg-amber-100 text-amber-800' :
                    'bg-emerald-100 text-emerald-800'
                  ]"
                >
                  {{ u.status === 'suspended' ? 'ถูกระงับ' : u.status === 'pending' ? 'รออนุมัติ' : 'ปกติ' }}
                </span>
              </td>
              <td class="py-4 px-6 text-xs text-slate-500">
                {{ new Date(u.created_at).toLocaleDateString('th-TH') }}
              </td>
              <td class="py-4 px-6 text-right">
                <button
                  v-if="u.role !== 'admin'"
                  @click="toggleSuspend(u)"
                  :class="[
                    'p-2 rounded-lg text-xs font-semibold transition-colors cursor-pointer',
                    u.status === 'suspended' ? 'text-emerald-600 hover:bg-emerald-50' : 'text-rose-600 hover:bg-rose-50'
                  ]"
                  :title="u.status === 'suspended' ? 'ยกเลิกระงับ' : 'ระงับบัญชี'"
                >
                  <CheckCircle v-if="u.status === 'suspended'" class="w-4 h-4" />
                  <Ban v-else class="w-4 h-4" />
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>
