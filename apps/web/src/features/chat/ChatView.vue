<script setup lang="ts">
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import { useRoute, RouterLink } from 'vue-router';
import { ArrowLeft, ImagePlus, Send, MessagesSquare, CalendarCheck } from 'lucide-vue-next';
import { chatApi } from './chat.api';
import { useAuthStore } from '@/features/auth/auth.store';
import { useSummaryStore } from '@/features/notifications/summary.store';
import { api } from '@/shared/lib/api';
import { useLiveEvent } from '@/shared/lib/liveEvents';
import type { ChatRoomDto, MessageDto } from '@/shared/types/models';
import { formatDate, formatRelativeTime, formatTime, toIsoDate } from '@/shared/lib/format';
import { bookingDetailPath, roleBase } from '@/shared/lib/paths';
import UserAvatar from '@/shared/ui/UserAvatar.vue';

// New messages arrive as live events; this slow poll only covers a dropped event stream.
const POLL_MS = 15_000;

const route = useRoute();
const auth = useAuthStore();
const summaryStore = useSummaryStore();

const rooms = ref<ChatRoomDto[]>([]);
const messages = ref<MessageDto[]>([]);
const isLoadingRooms = ref(true);
const draft = ref('');
const isSending = ref(false);
const error = ref<string | null>(null);
const scroller = ref<HTMLElement | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);
let pollTimer: ReturnType<typeof setInterval> | undefined;

const base = computed(() => roleBase(auth.user?.role));
const roomId = computed(() => route.params.roomId as string | undefined);
const activeRoom = computed(() => rooms.value.find((r) => r.id === roomId.value));
const myBubbleClass = computed(() => (auth.user?.role === 'sitter' ? 'bg-[#1E2238] text-white' : 'bg-[#FF7A59] text-white'));

async function loadRooms() {
  rooms.value = await chatApi.rooms();
  isLoadingRooms.value = false;
}

function appendMessages(incoming: MessageDto[]) {
  const known = new Set(messages.value.map((m) => m.id));
  const fresh = incoming.filter((m) => !known.has(m.id));
  if (fresh.length === 0) return;
  messages.value.push(...fresh);
  scrollToBottom();
}

async function loadNewMessages() {
  const id = roomId.value;
  if (!id) return;
  const fresh = await chatApi.messages(id, messages.value[messages.value.length - 1]?.createdAt);
  if (id === roomId.value) appendMessages(fresh);
}

async function scrollToBottom() {
  await nextTick();
  scroller.value?.scrollTo({ top: scroller.value.scrollHeight });
}

async function send(payload: { content?: string; imageUrl?: string }) {
  if (!roomId.value) return;
  isSending.value = true;
  error.value = null;
  try {
    appendMessages([await chatApi.send(roomId.value, payload)]);
    loadRooms();
  } catch (e) {
    error.value = (e as Error).message;
  } finally {
    isSending.value = false;
  }
}

async function sendText() {
  const content = draft.value.trim();
  if (!content || isSending.value) return;
  draft.value = '';
  await send({ content });
}

async function onImagePicked(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;
  (event.target as HTMLInputElement).value = '';
  try {
    const { url } = await api.upload('chat-images', file);
    await send({ imageUrl: url });
  } catch (e) {
    error.value = (e as Error).message;
  }
}

// Show a date divider above the first message of each day.
function startsNewDay(index: number): boolean {
  if (index === 0) return true;
  return toIsoDate(messages.value[index].createdAt) !== toIsoDate(messages.value[index - 1].createdAt);
}

watch(
  roomId,
  async (id) => {
    messages.value = [];
    error.value = null;
    if (!id) return;
    const initial = await chatApi.messages(id);
    if (id !== roomId.value) return;
    messages.value = initial;
    scrollToBottom();
    // Opening the room marked its messages read.
    loadRooms();
    summaryStore.refresh();
  },
  { immediate: true },
);

useLiveEvent('message', async (message) => {
  // Fetching (rather than appending the pushed copy) also marks it read on the server.
  if (message.roomId === roomId.value) await loadNewMessages();
  await loadRooms();
  summaryStore.refresh();
});

onMounted(() => {
  loadRooms();
  pollTimer = setInterval(() => {
    loadNewMessages();
    loadRooms();
  }, POLL_MS);
});

onUnmounted(() => clearInterval(pollTimer));
</script>

<template>
  <div class="bg-white rounded-2xl border border-slate-100 shadow-sm overflow-hidden flex h-[calc(100dvh-6.5rem)] lg:h-[calc(100dvh-3rem)]">
    <!-- Room list -->
    <aside :class="['w-full md:w-80 md:border-r border-slate-100 flex-col min-h-0', roomId ? 'hidden md:flex' : 'flex']">
      <div class="px-5 h-16 flex items-center border-b border-slate-100 shrink-0">
        <h1 class="text-lg font-bold text-slate-800">ข้อความ</h1>
      </div>
      <div class="flex-1 overflow-y-auto">
        <div v-if="isLoadingRooms" class="text-center py-10 text-slate-400 text-sm">กำลังโหลด...</div>
        <div v-else-if="rooms.length === 0" class="text-center px-6 py-12 text-sm text-slate-500">
          <MessagesSquare class="w-10 h-10 mx-auto text-slate-300 mb-2" />
          ยังไม่มีบทสนทนา
          <p v-if="auth.user?.role === 'owner'" class="text-xs text-slate-400 mt-1">เริ่มแชทได้จากหน้าโปรไฟล์ผู้รับฝากเลี้ยง หรือหน้ารายละเอียดการจอง</p>
        </div>
        <RouterLink
          v-for="room in rooms"
          :key="room.id"
          :to="`${base}/chat/${room.id}`"
          class="flex items-center gap-3 px-4 py-3 hover:bg-slate-50 transition"
          :class="{ 'bg-slate-50': room.id === roomId }"
        >
          <UserAvatar :name="room.other.name" :src="room.other.avatarUrl" size-class="w-11 h-11 text-base" />
          <div class="flex-1 min-w-0">
            <div class="flex items-center justify-between gap-2">
              <p class="text-sm font-semibold text-slate-800 truncate">{{ room.other.name }}</p>
              <span class="text-[11px] text-slate-400 shrink-0">{{ formatRelativeTime(room.lastMessageTime) }}</span>
            </div>
            <div class="flex items-center justify-between gap-2 mt-0.5">
              <p class="text-xs truncate" :class="room.unreadCount ? 'text-slate-800 font-medium' : 'text-slate-500'">
                {{ room.lastMessage || 'เริ่มบทสนทนาได้เลย' }}
              </p>
              <span v-if="room.unreadCount" class="min-w-5 h-5 px-1.5 rounded-full bg-[#E63946] text-white text-[11px] font-bold flex items-center justify-center shrink-0">
                {{ room.unreadCount }}
              </span>
            </div>
          </div>
        </RouterLink>
      </div>
    </aside>

    <!-- Conversation -->
    <section :class="['flex-1 min-w-0 min-h-0 flex-col', roomId ? 'flex' : 'hidden md:flex']">
      <div v-if="!roomId" class="flex-1 flex flex-col items-center justify-center text-slate-400 text-sm">
        <MessagesSquare class="w-12 h-12 text-slate-200 mb-2" />
        เลือกบทสนทนาเพื่อเริ่มแชท
      </div>

      <template v-else>
        <div class="px-4 h-16 flex items-center gap-3 border-b border-slate-100 shrink-0">
          <RouterLink :to="`${base}/chat`" class="md:hidden p-1.5 -ml-1.5 rounded-lg text-slate-500 hover:bg-slate-100" aria-label="กลับ">
            <ArrowLeft class="w-5 h-5" />
          </RouterLink>
          <template v-if="activeRoom">
            <UserAvatar :name="activeRoom.other.name" :src="activeRoom.other.avatarUrl" />
            <div class="flex-1 min-w-0">
              <p class="text-sm font-semibold text-slate-800 truncate">{{ activeRoom.other.name }}</p>
              <p class="text-xs text-slate-400">{{ activeRoom.other.role === 'sitter' ? 'ผู้รับฝากเลี้ยง' : 'เจ้าของแมว' }}</p>
            </div>
            <RouterLink
              v-if="activeRoom.bookingId"
              :to="bookingDetailPath(auth.user?.role, activeRoom.bookingId)"
              class="flex items-center gap-1.5 text-xs font-semibold text-slate-500 hover:text-slate-700 px-2.5 py-1.5 rounded-lg hover:bg-slate-100 shrink-0"
            >
              <CalendarCheck class="w-4 h-4" /> <span class="hidden sm:inline">ดูการจอง</span>
            </RouterLink>
          </template>
        </div>

        <div ref="scroller" class="flex-1 overflow-y-auto px-4 py-4 space-y-2 bg-[#FDFBF9]">
          <template v-for="(m, i) in messages" :key="m.id">
            <p v-if="startsNewDay(i)" class="text-center text-[11px] text-slate-400 py-2">{{ formatDate(m.createdAt) }}</p>
            <div class="flex" :class="m.senderId === auth.user?.id ? 'justify-end' : 'justify-start'">
              <div class="max-w-[78%]">
                <div
                  class="rounded-2xl px-3.5 py-2 text-sm whitespace-pre-wrap break-words"
                  :class="m.senderId === auth.user?.id ? [myBubbleClass, 'rounded-br-md'] : 'bg-white border border-slate-100 text-slate-800 rounded-bl-md'"
                >
                  <a v-if="m.imageUrl" :href="m.imageUrl" target="_blank" rel="noopener">
                    <img :src="m.imageUrl" alt="รูปภาพในแชท" class="rounded-lg max-h-60 object-cover" :class="{ 'mb-1': m.content }" />
                  </a>
                  <span v-if="m.content">{{ m.content }}</span>
                </div>
                <p class="text-[10px] text-slate-400 mt-0.5 px-1" :class="{ 'text-right': m.senderId === auth.user?.id }">
                  {{ formatTime(m.createdAt) }}
                </p>
              </div>
            </div>
          </template>
        </div>

        <p v-if="error" class="px-4 pt-2 text-xs text-red-500">{{ error }}</p>
        <form class="p-3 border-t border-slate-100 flex items-end gap-2 shrink-0" @submit.prevent="sendText">
          <input ref="fileInput" type="file" accept="image/*" class="hidden" @change="onImagePicked" />
          <button
            type="button"
            class="p-2.5 rounded-xl text-slate-500 hover:bg-slate-100 shrink-0"
            aria-label="แนบรูปภาพ"
            :disabled="isSending"
            @click="fileInput?.click()"
          >
            <ImagePlus class="w-5 h-5" />
          </button>
          <textarea
            v-model="draft"
            rows="1"
            aria-label="ข้อความ"
            placeholder="พิมพ์ข้อความ..."
            class="flex-1 resize-none px-3.5 py-2.5 rounded-xl border border-slate-200 text-sm focus:outline-none focus:border-[#FF7A59] max-h-32"
            @keydown.enter.exact.prevent="sendText"
          />
          <button
            type="submit"
            :disabled="!draft.trim() || isSending"
            class="p-2.5 rounded-xl bg-[#FF7A59] hover:bg-[#E05638] text-white disabled:opacity-40 shrink-0"
            aria-label="ส่ง"
          >
            <Send class="w-5 h-5" />
          </button>
        </form>
      </template>
    </section>
  </div>
</template>
