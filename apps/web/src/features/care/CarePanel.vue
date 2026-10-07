<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue';
import { LogIn, LogOut, Camera, Check, MapPin, ClipboardCheck, ImagePlus, X } from 'lucide-vue-next';
import { careApi } from './care.api';
import { api } from '@/shared/lib/api';
import type { AttendanceDto, BookingCareDto, BookingDto, ChecklistItemDto } from '@/shared/types/models';
import { formatDate, formatTime, toIsoDate } from '@/shared/lib/format';
import PhotoStrip from '@/shared/ui/PhotoStrip.vue';

const props = defineProps<{ booking: BookingDto; care: BookingCareDto; isSitter: boolean }>();
const emit = defineEmits<{ changed: [care: BookingCareDto] }>();

const busy = ref(false);
const busyItemId = ref<string | null>(null);
const error = ref<string | null>(null);
const photoInput = ref<HTMLInputElement | null>(null);
const photoItemId = ref<string | null>(null);

const today = toIsoDate(new Date());
const openVisit = computed(() => props.care.attendance.find((a) => !a.checkOutTime));
const withinDates = computed(() => today >= props.booking.startDate && today <= props.booking.endDate);
const canCheckIn = computed(
  () => props.isSitter && !openVisit.value && withinDates.value && ['confirmed', 'in_progress'].includes(props.booking.status),
);
const canEditChecklist = computed(() => props.isSitter && props.booking.status === 'in_progress');

// One block per day, newest first: that day's checklist plus its visits.
const days = computed(() => {
  const dates = new Set([
    ...props.care.checklists.map((c) => c.date),
    ...props.care.attendance.map((a) => toIsoDate(a.checkInTime)),
  ]);
  return [...dates]
    .sort((a, b) => b.localeCompare(a))
    .map((date) => {
      const checklist = props.care.checklists.find((c) => c.date === date);
      return {
        date,
        checklist,
        done: checklist?.items.filter((i) => i.isCompleted).length ?? 0,
        visits: props.care.attendance.filter((a) => toIsoDate(a.checkInTime) === date).reverse(),
      };
    });
});

function visitPhotos(visit: AttendanceDto): string[] {
  return [visit.checkinPhotoUrl, visit.checkoutPhotoUrl].filter((url): url is string => !!url);
}

async function run(action: () => Promise<BookingCareDto>) {
  error.value = null;
  try {
    emit('changed', await action());
  } catch (e) {
    error.value = (e as Error).message;
  }
}

function currentPosition(): Promise<{ lat: number; lng: number } | null> {
  if (!navigator.geolocation) return Promise.resolve(null);
  return new Promise((resolve) => {
    navigator.geolocation.getCurrentPosition(
      (pos) => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
      () => resolve(null),
      { timeout: 5000, maximumAge: 60000 },
    );
  });
}

// Check-in / check-out open a small form for an optional photo and a note to the owner.
const visitMode = ref<'in' | 'out' | null>(null);
const visitNote = ref('');
const visitPhoto = ref<{ file: File; preview: string } | null>(null);

function openVisitForm(mode: 'in' | 'out') {
  visitMode.value = mode;
  visitNote.value = '';
  clearVisitPhoto();
}

function clearVisitPhoto() {
  if (visitPhoto.value) URL.revokeObjectURL(visitPhoto.value.preview);
  visitPhoto.value = null;
}

function onVisitPhotoPicked(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  input.value = '';
  if (!file) return;
  clearVisitPhoto();
  visitPhoto.value = { file, preview: URL.createObjectURL(file) };
}

async function submitVisit() {
  const mode = visitMode.value;
  if (!mode) return;
  busy.value = true;
  await run(async () => {
    const photoUrl = visitPhoto.value ? (await api.upload('checklist-photos', visitPhoto.value.file)).url : undefined;
    const note = visitNote.value.trim() || undefined;
    if (mode === 'out') return careApi.checkOut(props.booking.id, { photoUrl, note });
    const position = await currentPosition();
    return careApi.checkIn(props.booking.id, { ...position, photoUrl, note });
  });
  busy.value = false;
  if (!error.value) {
    visitMode.value = null;
    clearVisitPhoto();
  }
}

onUnmounted(clearVisitPhoto);

async function toggle(item: ChecklistItemDto) {
  if (!canEditChecklist.value) return;
  busyItemId.value = item.id;
  await run(() => careApi.updateItem(item.id, { isCompleted: !item.isCompleted }));
  busyItemId.value = null;
}

function pickPhoto(item: ChecklistItemDto) {
  photoItemId.value = item.id;
  photoInput.value?.click();
}

async function onPhotoPicked(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  const itemId = photoItemId.value;
  input.value = '';
  if (!file || !itemId) return;
  busyItemId.value = itemId;
  await run(async () => {
    const { url } = await api.upload('checklist-photos', file);
    return careApi.updateItem(itemId, { photoUrl: url, isCompleted: true });
  });
  busyItemId.value = null;
}
</script>

<template>
  <section class="bg-white rounded-2xl border border-slate-100 shadow-sm p-5">
    <div class="flex flex-wrap items-start justify-between gap-3 mb-4">
      <div>
        <h2 class="font-semibold text-slate-800 flex items-center gap-2"><ClipboardCheck class="w-5 h-5 text-[#FF7A59]" /> บันทึกการดูแล</h2>
        <p class="text-xs text-slate-500 mt-0.5">เช็กอิน-เช็กเอาต์ และเช็กลิสต์งานประจำวัน</p>
      </div>
      <div v-if="isSitter && !visitMode" class="flex gap-2">
        <button
          v-if="openVisit"
          class="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-[#1E2238] text-white text-sm font-semibold"
          @click="openVisitForm('out')"
        >
          <LogOut class="w-4 h-4" /> เช็กเอาต์
        </button>
        <button
          v-else-if="canCheckIn"
          class="flex items-center gap-1.5 px-4 py-2 rounded-xl bg-green-600 hover:bg-green-700 text-white text-sm font-semibold"
          @click="openVisitForm('in')"
        >
          <LogIn class="w-4 h-4" /> เช็กอิน
        </button>
      </div>
    </div>

    <form v-if="visitMode" class="rounded-xl border border-slate-200 p-4 mb-4 space-y-3" @submit.prevent="submitVisit">
      <p class="text-sm font-semibold text-slate-800">
        {{ visitMode === 'in' ? 'เช็กอิน · ถ่ายรูปตอนมาถึงให้เจ้าของอุ่นใจ' : 'เช็กเอาต์ · สรุปรอบนี้ให้เจ้าของ' }}
      </p>
      <div class="flex items-start gap-3">
        <div v-if="visitPhoto" class="relative shrink-0">
          <img :src="visitPhoto.preview" alt="รูปที่จะแนบ" class="w-20 h-20 rounded-lg object-cover" />
          <button
            type="button"
            class="absolute -top-1.5 -right-1.5 w-5 h-5 rounded-full bg-slate-700 text-white flex items-center justify-center"
            aria-label="ลบรูป"
            @click="clearVisitPhoto"
          >
            <X class="w-3 h-3" />
          </button>
        </div>
        <label
          v-else
          class="w-20 h-20 shrink-0 rounded-lg border-2 border-dashed border-slate-200 text-slate-400 hover:border-[#FF7A59] hover:text-[#FF7A59] flex flex-col items-center justify-center cursor-pointer text-[10px]"
        >
          <ImagePlus class="w-5 h-5" />
          แนบรูป
          <input type="file" accept="image/*" capture="environment" class="hidden" @change="onVisitPhotoPicked" />
        </label>
        <textarea
          v-model="visitNote"
          rows="3"
          maxlength="500"
          aria-label="บันทึกถึงเจ้าของ"
          :placeholder="visitMode === 'in' ? 'เช่น มาถึงแล้ว น้องๆ มารอที่ประตูเลย (ไม่บังคับ)' : 'เช่น ให้อาหารแล้ว น้องกินหมด เล่นด้วยกัน 20 นาที (ไม่บังคับ)'"
          class="flex-1 min-w-0 px-3 py-2 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59]"
        />
      </div>
      <div class="flex justify-end gap-2">
        <button type="button" class="px-4 py-2 rounded-xl text-sm text-slate-500 hover:bg-slate-100" @click="visitMode = null">ยกเลิก</button>
        <button
          type="submit"
          :disabled="busy"
          class="px-4 py-2 rounded-xl text-white text-sm font-semibold disabled:opacity-50"
          :class="visitMode === 'in' ? 'bg-green-600 hover:bg-green-700' : 'bg-[#1E2238]'"
        >
          {{ busy ? 'กำลังบันทึก...' : visitMode === 'in' ? 'ยืนยันเช็กอิน' : 'ยืนยันเช็กเอาต์' }}
        </button>
      </div>
    </form>

    <div v-if="openVisit" class="flex items-center gap-2 text-sm text-green-700 bg-green-50 border border-green-100 rounded-xl px-3.5 py-2.5 mb-4">
      <span class="relative flex w-2.5 h-2.5">
        <span class="absolute inline-flex h-full w-full rounded-full bg-green-500 opacity-75 animate-ping" />
        <span class="relative inline-flex w-2.5 h-2.5 rounded-full bg-green-500" />
      </span>
      กำลังดูแลน้องอยู่ · เช็กอินเมื่อ {{ formatTime(openVisit.checkInTime) }} น.
    </div>

    <p v-if="error" class="text-xs text-red-500 mb-3">{{ error }}</p>
    <input ref="photoInput" type="file" accept="image/*" capture="environment" class="hidden" @change="onPhotoPicked" />

    <div v-if="days.length === 0" class="text-center py-8 bg-slate-50 rounded-xl text-sm text-slate-500">
      <template v-if="isSitter">กดเช็กอินเมื่อไปถึง ระบบจะสร้างเช็กลิสต์งานประจำวันให้</template>
      <template v-else>พี่เลี้ยงยังไม่ได้เริ่มบันทึกการดูแล</template>
    </div>

    <div v-else class="space-y-5">
      <div v-for="day in days" :key="day.date">
        <div class="flex items-center justify-between mb-2">
          <p class="text-sm font-semibold text-slate-700">{{ day.date === today ? 'วันนี้' : formatDate(day.date) }}</p>
          <p v-if="day.checklist" class="text-xs font-medium" :class="day.done === day.checklist.items.length ? 'text-green-600' : 'text-slate-500'">
            {{ day.done }}/{{ day.checklist.items.length }} งาน
          </p>
        </div>
        <div v-if="day.checklist" class="h-1.5 rounded-full bg-slate-100 overflow-hidden mb-3">
          <div class="h-full rounded-full bg-green-500 transition-all" :style="{ width: `${(day.done / day.checklist.items.length) * 100}%` }" />
        </div>

        <ul v-if="day.checklist" class="space-y-1.5">
          <li
            v-for="item in day.checklist.items"
            :key="item.id"
            class="flex items-center gap-3 rounded-xl border px-3 py-2.5"
            :class="item.isCompleted ? 'border-green-100 bg-green-50/50' : 'border-slate-100'"
          >
            <button
              :disabled="!canEditChecklist || busyItemId === item.id"
              class="w-6 h-6 rounded-lg border-2 flex items-center justify-center shrink-0 transition"
              :class="[
                item.isCompleted ? 'bg-green-500 border-green-500 text-white' : 'border-slate-300 bg-white',
                canEditChecklist ? 'cursor-pointer' : 'cursor-default',
              ]"
              :aria-label="item.isCompleted ? 'ยกเลิกเครื่องหมาย' : 'ทำเครื่องหมายว่าเสร็จ'"
              @click="toggle(item)"
            >
              <Check v-if="item.isCompleted" class="w-4 h-4" />
            </button>
            <div class="flex-1 min-w-0">
              <p class="text-sm" :class="item.isCompleted ? 'text-slate-500 line-through' : 'text-slate-800'">{{ item.taskName }}</p>
              <p v-if="item.completedAt" class="text-[11px] text-slate-400">เสร็จเมื่อ {{ formatTime(item.completedAt) }} น.</p>
            </div>
            <a v-if="item.photoUrl" :href="item.photoUrl" target="_blank" rel="noopener" class="shrink-0">
              <img :src="item.photoUrl" :alt="item.taskName" class="w-11 h-11 rounded-lg object-cover" />
            </a>
            <button
              v-if="canEditChecklist"
              :disabled="busyItemId === item.id"
              class="p-2 rounded-lg text-slate-400 hover:text-[#FF7A59] hover:bg-[#FFF0EB] shrink-0"
              aria-label="แนบรูป"
              @click="pickPhoto(item)"
            >
              <Camera class="w-4 h-4" />
            </button>
          </li>
        </ul>

        <ul v-if="day.visits.length" class="mt-2 space-y-1">
          <li v-for="visit in day.visits" :key="visit.id" class="flex flex-wrap items-center gap-x-2 gap-y-0.5 text-xs text-slate-500 pl-1">
            <LogIn class="w-3.5 h-3.5 text-green-600" />
            เช็กอิน {{ formatTime(visit.checkInTime) }}
            <template v-if="visit.checkOutTime">– เช็กเอาต์ {{ formatTime(visit.checkOutTime) }}</template>
            <span v-else class="text-green-600 font-medium">(กำลังดูแล)</span>
            <a
              v-if="visit.locationLat && visit.locationLng"
              :href="`https://maps.google.com/?q=${visit.locationLat},${visit.locationLng}`"
              target="_blank"
              rel="noopener"
              class="flex items-center gap-0.5 text-blue-500 hover:underline"
            >
              <MapPin class="w-3 h-3" /> ตำแหน่ง
            </a>
            <span v-if="visit.note" class="basis-full pl-5 text-slate-400">“{{ visit.note }}”</span>
            <PhotoStrip v-if="visitPhotos(visit).length" :photos="visitPhotos(visit)" alt="รูปตอนเช็กอิน/เช็กเอาต์" size-class="w-14 h-14" class="basis-full pl-5 mt-1" />
          </li>
        </ul>
      </div>
    </div>
  </section>
</template>
