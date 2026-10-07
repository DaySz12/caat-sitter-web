import { defineStore } from 'pinia';
import { ref } from 'vue';
import { notificationsApi } from './notifications.api';
import type { UserSummaryDto } from '@/shared/types/models';

// Sidebar badge counts + dashboard stats. The sidebar refreshes it on every navigation and
// every 30 s; pages that change a count (reading a notification, sending a message) refresh it too.
export const useSummaryStore = defineStore('summary', () => {
  const summary = ref<UserSummaryDto | null>(null);

  async function refresh() {
    try {
      summary.value = await notificationsApi.summary();
    } catch {
      // Badges are best-effort; keep the last known counts.
    }
  }

  function clear() {
    summary.value = null;
  }

  return { summary, refresh, clear };
});
