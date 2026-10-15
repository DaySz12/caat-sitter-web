<script setup lang="ts">
import { ref, computed } from 'vue';
import {
  X,
  PhoneCall,
  HeartPulse,
  Navigation,
  ExternalLink,
  MessageCircle,
  MapPin,
  Send,
  CheckCircle2,
  AlertTriangle,
  User,
  ShieldAlert
} from 'lucide-vue-next';
import type { BookingDto } from '@/shared/types/models';
import { chatApi } from '@/features/chat/chat.api';

const props = defineProps<{
  isOpen: boolean;
  booking: BookingDto;
  isSitter: boolean;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'open-chat'): void;
}>();

const isSendingSos = ref(false);
const sosSentSuccess = ref(false);
const sosErrorMessage = ref<string | null>(null);

const ownerPhone = computed(() => props.booking.owner?.phone);
const ownerName = computed(() => props.booking.owner?.name || 'เจ้าของแมว');
const secondaryPhone = computed(
  () => props.booking.emergencyContactPhone || props.booking.owner?.secondaryPhone
);
const emergencyContactName = computed(
  () => props.booking.owner?.emergencyContactName || 'ผู้ติดต่อฉุกเฉินสำรอง'
);

const sitterPhone = computed(() => props.booking.sitter?.phone);
const sitterName = computed(() => props.booking.sitter?.name || 'พี่เลี้ยงแมว');

const preferredClinic = computed(() => props.booking.preferredVetClinic);

// Additional clinic info from specific cats
const catClinics = computed(() => {
  return props.booking.cats
    .filter(
      (c) =>
        c.emergencyVetClinic &&
        c.emergencyVetClinic.toLowerCase() !== preferredClinic.value?.toLowerCase()
    )
    .map((c) => ({
      catName: c.name,
      clinic: c.emergencyVetClinic!,
    }));
});

// Nearby 24/7 animal hospitals Google Maps search link
const nearbyVetSearchUrl = computed(() => {
  const query = props.booking.serviceAddress
    ? `โรงพยาบาลสัตว์ 24 ชั่วโมง ${props.booking.serviceAddress}`
    : 'โรงพยาบาลสัตว์ 24 ชั่วโมง ใกล้ฉัน';
  return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
});

// Major 24-hour veterinary hospital emergency network in Thailand
const major24hVets = [
  {
    name: 'โรงพยาบาลสัตว์ทองหล่อ 24 ชม.',
    subtext: 'ศูนย์รับส่งสัตว์ป่วยฉุกเฉินและรถพยาบาลสัตว์ 24 ชั่วโมง ทุกสาขา',
    phone: '02-079-9999',
    rawPhone: '020799999',
    badge: 'ตลอด 24 ชม.',
    mapQuery: 'โรงพยาบาลสัตว์ทองหล่อ 24 ชม.',
  },
  {
    name: 'โรงพยาบาลสัตว์ ม.เกษตรศาสตร์ บางเขน',
    subtext: 'ศูนย์ฉุกเฉินและส่งต่อสัตว์ป่วยวิกฤต 24 ชั่วโมง (ER & Critical Care)',
    phone: '02-797-1900',
    rawPhone: '027971900',
    badge: 'ฉุกเฉิน ม.เกษตร',
    mapQuery: 'โรงพยาบาลสัตว์ มหาวิทยาลัยเกษตรศาสตร์ บางเขน',
  },
  {
    name: 'โรงพยาบาลสัตว์เล็ก จุฬาลงกรณ์มหาวิทยาลัย',
    subtext: 'หน่วยฉุกเฉินและบำบัดวิกฤตสัตว์ป่วยตลอด 24 ชั่วโมง (สยามสแควร์)',
    phone: '02-218-9751',
    rawPhone: '022189751',
    badge: 'ฉุกเฉิน จุฬาฯ',
    mapQuery: 'โรงพยาบาลสัตว์เล็ก จุฬาลงกรณ์มหาวิทยาลัย',
  },
  {
    name: 'โรงพยาบาลสัตว์สุวรรณชาด 24 ชม.',
    subtext: 'บริการตรวจรักษาฉุกเฉิน 24 ชั่วโมง โครงการส่วนพระองค์ ถนนรามคำแหง',
    phone: '02-729-5706',
    rawPhone: '027295706',
    badge: 'รามคำแหง',
    mapQuery: 'โรงพยาบาลสัตว์สุวรรณชาด',
  },
];

async function handleSendSosChatAlert() {
  const targetUserId = props.isSitter ? props.booking.ownerId : props.booking.sitterId;
  if (!targetUserId) return;

  isSendingSos.value = true;
  sosErrorMessage.value = null;

  try {
    const room = await chatApi.start(targetUserId, props.booking.id);
    const senderRole = props.isSitter ? 'พี่เลี้ยง' : 'เจ้าของแมว';
    const catNames = props.booking.cats.map((c) => c.name).join(', ') || 'น้องแมว';

    await chatApi.send(room.id, {
      content: `[แจ้งเหตุด่วน SOS] ${senderRole} ได้ส่งข้อความแจ้งเหตุฉุกเฉินเกี่ยวกับ ${catNames} (รหัสการจอง #${props.booking.bookingCode}) กรุณาตรวจสอบหรือติดต่อกลับโดยเร็วที่สุด`,
    });

    sosSentSuccess.value = true;
  } catch (err: any) {
    sosErrorMessage.value =
      err?.message || 'ไม่สามารถส่งข้อความแจ้งเตือนได้ กรุณาโทรติดต่อโดยตรง';
  } finally {
    isSendingSos.value = false;
  }
}
</script>

<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 bg-slate-950/60 backdrop-blur-xs z-50 flex items-center justify-center p-3 sm:p-4 overflow-y-auto"
    @click.self="emit('close')"
  >
    <div
      class="bg-white rounded-2xl max-w-2xl w-full shadow-xl border border-slate-200 overflow-hidden my-6"
    >
      <!-- Modal Header -->
      <div class="px-6 py-5 bg-slate-900 text-white flex items-start justify-between">
        <div>
          <div class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full bg-rose-500/20 text-rose-300 border border-rose-500/30 text-xs font-medium mb-1.5">
            <ShieldAlert class="w-3.5 h-3.5 text-rose-400" />
            ศูนย์ความช่วยเหลือฉุกเฉิน
          </div>
          <h3 class="font-bold text-lg text-white flex items-center gap-2">
            <PhoneCall class="w-5 h-5 text-rose-400" />
            สายด่วนฉุกเฉินและสัตวแพทย์
          </h3>
          <p class="text-xs text-slate-300 mt-1 max-w-lg leading-relaxed">
            ติดต่อด่วนบุคคลที่เกี่ยวข้อง หรือค้นหาโรงพยาบาลสัตว์ 24 ชั่วโมงที่ใกล้ที่สุด
          </p>
        </div>
        <button
          type="button"
          @click="emit('close')"
          class="text-slate-400 hover:text-white p-1.5 rounded-lg hover:bg-slate-800 transition cursor-pointer"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Modal Body -->
      <div class="p-5 sm:p-6 space-y-5 max-h-[75vh] overflow-y-auto">
        <!-- Booking context banner -->
        <div class="bg-slate-50 border border-slate-200 rounded-xl p-3 flex flex-wrap items-center justify-between gap-2 text-xs">
          <div class="flex items-center gap-2 text-slate-700">
            <span class="text-slate-500">รหัสการจอง:</span>
            <strong class="font-mono text-slate-900">#{{ booking.bookingCode }}</strong>
            <span class="text-slate-300">·</span>
            <span class="text-slate-500">สัตว์เลี้ยง:</span>
            <strong class="text-slate-800">{{ booking.cats.map(c => c.name).join(', ') || '-' }}</strong>
          </div>
          <div v-if="booking.serviceAddress" class="text-slate-500 text-[11px] truncate max-w-xs flex items-center gap-1">
            <MapPin class="w-3.5 h-3.5 text-slate-400 shrink-0" />
            <span class="truncate">{{ booking.serviceAddress }}</span>
          </div>
        </div>

        <!-- Section 1: One-Click Direct Emergency Calls -->
        <div>
          <h4 class="text-xs font-bold uppercase tracking-wider text-slate-600 mb-2.5 flex items-center gap-1.5">
            <User class="w-3.5 h-3.5 text-slate-500" />
            ผู้ติดต่อโดยตรง
          </h4>

          <div class="grid sm:grid-cols-2 gap-3">
            <!-- If Sitter viewing: Show Owner Primary & Secondary -->
            <template v-if="isSitter">
              <!-- Owner Primary -->
              <div class="p-3.5 rounded-xl border border-slate-200 bg-white hover:border-slate-300 transition space-y-2">
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-slate-600">เจ้าของแมว (หลัก)</span>
                  <span class="text-[11px] px-2 py-0.5 rounded bg-slate-100 text-slate-600 font-medium">เจ้าของการจอง</span>
                </div>
                <p class="text-sm font-bold text-slate-900 truncate">{{ ownerName }}</p>
                <div v-if="ownerPhone" class="pt-1">
                  <a
                    :href="`tel:${ownerPhone}`"
                    class="w-full py-2 px-3 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white font-medium text-xs flex items-center justify-center gap-2 transition cursor-pointer"
                  >
                    <PhoneCall class="w-3.5 h-3.5" />
                    โทร {{ ownerPhone }}
                  </a>
                </div>
                <div v-else class="text-xs text-slate-400 italic py-1">
                  ไม่ได้ระบุเบอร์โทรศัพท์
                </div>
              </div>

              <!-- Owner Secondary / Emergency Contact -->
              <div class="p-3.5 rounded-xl border border-amber-200 bg-amber-50/40 space-y-2">
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-amber-900">{{ emergencyContactName }}</span>
                  <span class="text-[11px] px-2 py-0.5 rounded bg-amber-100 text-amber-800 font-medium">เบอร์สำรอง</span>
                </div>
                <p class="text-sm font-bold text-slate-900">
                  {{ secondaryPhone || 'ไม่มีการระบุเบอร์สำรอง' }}
                </p>
                <div v-if="secondaryPhone" class="pt-1">
                  <a
                    :href="`tel:${secondaryPhone}`"
                    class="w-full py-2 px-3 rounded-lg bg-amber-700 hover:bg-amber-800 text-white font-medium text-xs flex items-center justify-center gap-2 transition cursor-pointer"
                  >
                    <PhoneCall class="w-3.5 h-3.5" />
                    โทรเบอร์สำรอง
                  </a>
                </div>
                <div v-else class="text-xs text-amber-700/80 italic py-1">
                  ผู้ว่าจ้างไม่ได้ระบุเบอร์สำรองไว้
                </div>
              </div>
            </template>

            <!-- If Cat Owner viewing: Show Sitter & CatSitter Support -->
            <template v-else>
              <!-- Sitter Contact -->
              <div class="p-3.5 rounded-xl border border-slate-200 bg-white hover:border-slate-300 transition space-y-2">
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-slate-600">พี่เลี้ยงผู้ดูแล</span>
                  <span class="text-[11px] px-2 py-0.5 rounded bg-slate-100 text-slate-600 font-medium">ผู้รับฝากเลี้ยง</span>
                </div>
                <p class="text-sm font-bold text-slate-900 truncate">{{ sitterName }}</p>
                <div v-if="sitterPhone" class="pt-1">
                  <a
                    :href="`tel:${sitterPhone}`"
                    class="w-full py-2 px-3 rounded-lg bg-emerald-600 hover:bg-emerald-700 text-white font-medium text-xs flex items-center justify-center gap-2 transition cursor-pointer"
                  >
                    <PhoneCall class="w-3.5 h-3.5" />
                    โทรหาพี่เลี้ยง ({{ sitterPhone }})
                  </a>
                </div>
                <div v-else class="text-xs text-slate-400 italic py-1">
                  ไม่ได้ระบุเบอร์โทรศัพท์
                </div>
              </div>

              <!-- CatSitter Emergency Helpline -->
              <div class="p-3.5 rounded-xl border border-rose-200 bg-rose-50/30 space-y-2">
                <div class="flex items-center justify-between">
                  <span class="text-xs font-semibold text-rose-900">ฝ่ายสนับสนุน CatSitter</span>
                  <span class="text-[11px] px-2 py-0.5 rounded bg-rose-100 text-rose-800 font-medium">ศูนย์ช่วยเหลือ</span>
                </div>
                <p class="text-sm font-bold text-slate-900">02-098-7654</p>
                <div class="pt-1">
                  <a
                    href="tel:020987654"
                    class="w-full py-2 px-3 rounded-lg bg-rose-600 hover:bg-rose-700 text-white font-medium text-xs flex items-center justify-center gap-2 transition cursor-pointer"
                  >
                    <PhoneCall class="w-3.5 h-3.5" />
                    โทรสายด่วน CatSitter
                  </a>
                </div>
              </div>
            </template>
          </div>
        </div>

        <!-- Section 2: Preferred Cat Clinic -->
        <div v-if="preferredClinic || catClinics.length > 0">
          <h4 class="text-xs font-bold uppercase tracking-wider text-slate-600 mb-2.5 flex items-center gap-1.5">
            <HeartPulse class="w-3.5 h-3.5 text-blue-600" />
            คลินิกประจำตัวน้องแมว
          </h4>

          <div class="p-4 rounded-xl border border-blue-200 bg-blue-50/40 space-y-2.5">
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
              <div>
                <p class="text-xs text-blue-800">สถานพยาบาลที่เจ้าของระบุไว้</p>
                <p class="text-sm font-bold text-slate-900 mt-0.5">{{ preferredClinic || catClinics[0]?.clinic }}</p>
                <p class="text-[11px] text-blue-700 mt-0.5">มีประวัติการรักษาและวัคซีนเดิมของน้องแมว แนะนำให้ประสานงานที่นี่ก่อน</p>
              </div>
              <a
                :href="`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(preferredClinic || catClinics[0]?.clinic)}`"
                target="_blank"
                rel="noopener noreferrer"
                class="inline-flex items-center justify-center gap-1.5 px-3.5 py-2 rounded-lg bg-blue-600 hover:bg-blue-700 text-white font-medium text-xs transition cursor-pointer shrink-0"
              >
                <Navigation class="w-3.5 h-3.5" />
                เปิดแผนที่นำทาง
                <ExternalLink class="w-3 h-3 text-blue-200" />
              </a>
            </div>

            <!-- Extra clinics if cats have distinct clinics -->
            <div v-if="catClinics.length > 1" class="pt-2 border-t border-blue-200/60 space-y-1">
              <p class="text-xs font-medium text-blue-900">คลินิกประจำตัวแมวตัวอื่นในรายการนี้:</p>
              <div v-for="c in catClinics" :key="c.catName" class="flex items-center justify-between text-xs text-blue-800">
                <span>{{ c.catName }}: <strong>{{ c.clinic }}</strong></span>
                <a
                  :href="`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(c.clinic)}`"
                  target="_blank"
                  class="text-blue-600 hover:underline inline-flex items-center gap-1"
                >
                  แผนที่ <ExternalLink class="w-2.5 h-2.5" />
                </a>
              </div>
            </div>
          </div>
        </div>

        <!-- Section 3: Nearby 24/7 Animal Hospital Search -->
        <div>
          <h4 class="text-xs font-bold uppercase tracking-wider text-slate-600 mb-2.5 flex items-center gap-1.5">
            <Navigation class="w-3.5 h-3.5 text-slate-600" />
            ค้นหาโรงพยาบาลสัตว์ 24 ชม. ใกล้เคียง
          </h4>

          <div class="p-4 rounded-xl border border-slate-200 bg-slate-50 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
            <div class="space-y-0.5">
              <p class="text-sm font-semibold text-slate-800 flex items-center gap-1.5">
                <MapPin class="w-4 h-4 text-rose-600" />
                โรงพยาบาลสัตว์เปิด 24 ชั่วโมงในพื้นที่
              </p>
              <p class="text-xs text-slate-500 max-w-md">
                ค้นหาศูนย์ฉุกเฉินสัตวแพทย์ที่เปิดให้บริการรอบสถานที่ดูแลหรือตำแหน่งปัจจุบันบน Google Maps
              </p>
            </div>
            <a
              :href="nearbyVetSearchUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="w-full sm:w-auto px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white font-medium text-xs inline-flex items-center justify-center gap-1.5 transition cursor-pointer shrink-0"
            >
              <Navigation class="w-3.5 h-3.5" />
              ค้นหาบน Google Maps
              <ExternalLink class="w-3 h-3 text-rose-200" />
            </a>
          </div>
        </div>

        <!-- Section 4: Verified Major 24-hr Vet Hospitals Directory -->
        <div>
          <h4 class="text-xs font-bold uppercase tracking-wider text-slate-600 mb-2.5 flex items-center gap-1.5">
            <ShieldAlert class="w-3.5 h-3.5 text-slate-600" />
            เครือข่ายโรงพยาบาลสัตว์ 24 ชั่วโมง
          </h4>

          <div class="grid sm:grid-cols-2 gap-2.5">
            <div
              v-for="vet in major24hVets"
              :key="vet.name"
              class="p-3.5 rounded-xl border border-slate-200 bg-white hover:border-slate-300 transition flex flex-col justify-between"
            >
              <div>
                <div class="flex items-center justify-between gap-1 mb-1">
                  <span class="text-[10px] font-medium px-2 py-0.5 rounded bg-slate-100 text-slate-700">
                    {{ vet.badge }}
                  </span>
                  <a
                    :href="`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(vet.mapQuery)}`"
                    target="_blank"
                    class="text-[11px] text-slate-400 hover:text-slate-700 inline-flex items-center gap-1"
                  >
                    <MapPin class="w-3 h-3" /> แผนที่
                  </a>
                </div>
                <p class="text-xs font-bold text-slate-900 leading-snug">{{ vet.name }}</p>
                <p class="text-[11px] text-slate-500 mt-1 line-clamp-2 leading-relaxed">{{ vet.subtext }}</p>
              </div>

              <div class="mt-3 pt-2.5 border-t border-slate-100 flex items-center justify-between gap-2">
                <span class="text-xs font-mono font-bold text-slate-800">{{ vet.phone }}</span>
                <a
                  :href="`tel:${vet.rawPhone}`"
                  class="px-2.5 py-1 rounded bg-slate-100 hover:bg-slate-200 text-slate-800 text-xs font-medium inline-flex items-center gap-1.5 transition cursor-pointer"
                >
                  <PhoneCall class="w-3 h-3 text-emerald-600" /> โทรออก
                </a>
              </div>
            </div>
          </div>
        </div>

        <!-- Section 5: Instant SOS Message to Chat -->
        <div class="p-4 rounded-xl border border-rose-200 bg-rose-50/30 space-y-3">
          <div>
            <p class="text-xs font-bold text-slate-900 flex items-center gap-1.5">
              <Send class="w-3.5 h-3.5 text-rose-600" />
              แจ้งเตือนเหตุด่วนเข้าห้องแชท
            </p>
            <p class="text-xs text-slate-600 mt-0.5">
              ส่งข้อความแจ้งเตือนด่วนเข้าห้องแชทของ{{ isSitter ? 'เจ้าของแมว' : 'พี่เลี้ยง' }}เพื่อให้ติดต่อกลับโดยเร็วที่สุด
            </p>
          </div>

          <div v-if="sosSentSuccess" class="p-3 bg-emerald-50 border border-emerald-200 rounded-lg text-xs text-emerald-800 flex items-center justify-between gap-2">
            <div class="flex items-center gap-2">
              <CheckCircle2 class="w-4 h-4 text-emerald-600 shrink-0" />
              <span class="font-medium">ส่งข้อความแจ้งเตือนเข้าห้องแชทเรียบร้อยแล้ว</span>
            </div>
            <button
              type="button"
              @click="emit('open-chat'); emit('close');"
              class="px-2.5 py-1 bg-emerald-700 hover:bg-emerald-800 text-white rounded font-medium text-xs transition cursor-pointer flex items-center gap-1"
            >
              <MessageCircle class="w-3 h-3" /> เปิดแชท
            </button>
          </div>

          <div v-else class="flex flex-col sm:flex-row items-center gap-2">
            <button
              type="button"
              :disabled="isSendingSos"
              @click="handleSendSosChatAlert"
              class="w-full sm:w-auto px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-700 text-white font-medium text-xs inline-flex items-center justify-center gap-1.5 disabled:opacity-50 transition cursor-pointer"
            >
              <span v-if="isSendingSos" class="w-3.5 h-3.5 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Send v-else class="w-3.5 h-3.5" />
              {{ isSendingSos ? 'กำลังส่งข้อความ...' : 'ส่งข้อความด่วนเข้าแชท' }}
            </button>
            <p v-if="sosErrorMessage" class="text-xs text-rose-600 font-medium">
              {{ sosErrorMessage }}
            </p>
          </div>
        </div>
      </div>

      <!-- Modal Footer -->
      <div class="px-6 py-3.5 bg-slate-50 border-t border-slate-100 flex items-center justify-end text-xs text-slate-500">
        <button
          type="button"
          @click="emit('close')"
          class="px-4 py-2 bg-slate-200 hover:bg-slate-300 text-slate-700 rounded-lg font-medium transition cursor-pointer"
        >
          ปิด
        </button>
      </div>
    </div>
  </div>
</template>
