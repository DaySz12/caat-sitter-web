// Live events pushed by the API over Server-Sent Events (GET /api/events). One EventSource is
// shared by the whole tab; components subscribe with useLiveEvent(). Pages still poll slowly as
// a fallback, e.g. while the stream is reconnecting after an API restart.
import { onUnmounted } from 'vue';
import { API_URL, getToken } from './api';
import type { MessageDto, NotificationDto } from '@/shared/types/models';

export interface LiveEventMap {
  notification: NotificationDto;
  message: MessageDto;
  care: { bookingId: string };
}

type LiveEventType = keyof LiveEventMap;
type Handler<K extends LiveEventType> = (data: LiveEventMap[K]) => void;

const handlers: { [K in LiveEventType]: Set<Handler<K>> } = {
  notification: new Set(),
  message: new Set(),
  care: new Set(),
};

let source: EventSource | null = null;

export function connectLiveEvents() {
  const token = getToken();
  if (source || !token) return;

  source = new EventSource(`${API_URL}/api/events?access_token=${encodeURIComponent(token)}`);
  for (const type of Object.keys(handlers) as LiveEventType[]) {
    source.addEventListener(type, (event) => {
      const data: unknown = JSON.parse((event as MessageEvent<string>).data);
      (handlers[type] as Set<(data: unknown) => void>).forEach((handler) => handler(data));
    });
  }
}

export function disconnectLiveEvents() {
  source?.close();
  source = null;
}

/** Subscribes for the lifetime of the calling component. Call from setup(). */
export function useLiveEvent<K extends LiveEventType>(type: K, handler: Handler<K>) {
  const set = handlers[type] as Set<Handler<K>>;
  set.add(handler);
  onUnmounted(() => set.delete(handler));
}
