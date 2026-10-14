import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
      meta: { guestOnly: true },
    },
    {
      path: '/',
      component: () => import('../layouts/AdminLayout.vue'),
      meta: { requiresAdmin: true },
      children: [
        {
          path: '',
          redirect: '/dashboard',
        },
        {
          path: 'dashboard',
          name: 'Dashboard',
          component: () => import('../views/DashboardView.vue'),
        },
        {
          path: 'sitters/verification',
          name: 'SitterVerification',
          component: () => import('../views/SitterVerificationView.vue'),
        },
        {
          path: 'bookings',
          name: 'Bookings',
          component: () => import('../views/BookingsView.vue'),
        },
        {
          path: 'payments',
          name: 'PaymentVerifications',
          component: () => import('../views/PaymentVerificationsView.vue'),
        },
        {
          path: 'payouts',
          name: 'Payouts',
          component: () => import('../views/PayoutsView.vue'),
        },
        {
          path: 'users',
          name: 'Users',
          component: () => import('../views/UsersView.vue'),
        },
        {
          path: 'banners',
          name: 'Banners',
          component: () => import('../views/BannersView.vue'),
        },
        {
          path: 'settings',
          name: 'Settings',
          component: () => import('../views/SettingsView.vue'),
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/dashboard',
    },
  ],
});

router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore();
  const hasSession = await authStore.checkSession();

  if (to.meta.requiresAdmin && (!hasSession || authStore.profile?.role !== 'admin')) {
    next({ name: 'Login' });
  } else if (to.meta.guestOnly && hasSession && authStore.profile?.role === 'admin') {
    next({ name: 'Dashboard' });
  } else {
    next();
  }
});

export default router;
