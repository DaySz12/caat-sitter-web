import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '@/features/auth/auth.store';

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'Login', component: () => import('@/features/auth/LoginView.vue'), meta: { guestOnly: true } },
    {
      path: '/signup/owner',
      name: 'SignupOwner',
      component: () => import('@/features/auth/SignupView.vue'),
      props: { role: 'owner' },
      meta: { guestOnly: true },
    },
    {
      path: '/signup/sitter',
      name: 'SignupSitter',
      component: () => import('@/features/auth/SignupView.vue'),
      props: { role: 'sitter' },
      meta: { guestOnly: true },
    },

    {
      path: '/owner',
      component: () => import('@/app/layouts/OwnerLayout.vue'),
      meta: { requiresAuth: true, role: 'owner' },
      children: [
        { path: '', name: 'OwnerHome', component: () => import('@/features/dashboard/OwnerHomeView.vue') },
        { path: 'cats', name: 'MyCats', component: () => import('@/features/cats/MyCatsView.vue') },
        { path: 'cats/new', name: 'CatNew', component: () => import('@/features/cats/CatFormView.vue') },
        { path: 'cats/:id/edit', name: 'CatEdit', component: () => import('@/features/cats/CatFormView.vue') },
        { path: 'sitters', name: 'SitterSearch', component: () => import('@/features/sitters/SitterSearchView.vue') },
        { path: 'sitters/:id', name: 'SitterDetail', component: () => import('@/features/sitters/SitterDetailView.vue') },
        { path: 'booking/new', name: 'BookingWizard', component: () => import('@/features/booking/BookingWizardView.vue') },
        { path: 'bookings', name: 'BookingHistory', component: () => import('@/features/booking/BookingHistoryView.vue') },
        { path: 'profile', name: 'OwnerProfile', component: () => import('@/features/profile/OwnerProfileView.vue') },
      ],
    },

    {
      path: '/sitter',
      component: () => import('@/app/layouts/SitterLayout.vue'),
      meta: { requiresAuth: true, role: 'sitter' },
      children: [
        { path: '', name: 'SitterHome', component: () => import('@/features/dashboard/SitterHomeView.vue') },
        { path: 'jobs', name: 'SitterJobs', component: () => import('@/features/booking/SitterJobsView.vue') },
        { path: 'requests', name: 'BookingRequests', component: () => import('@/features/booking/BookingRequestsView.vue') },
        { path: 'profile', name: 'SitterProfile', component: () => import('@/features/profile/SitterProfileView.vue') },
      ],
    },

    { path: '/', redirect: '/login' },
    { path: '/:pathMatch(.*)*', redirect: '/login' },
  ],
});

router.beforeEach(async (to) => {
  const auth = useAuthStore();
  if (auth.isLoading) {
    await auth.checkSession();
  }

  const isAuthed = !!auth.user;

  if (to.meta.requiresAuth && !isAuthed) {
    return { name: 'Login' };
  }

  if (to.meta.requiresAuth && to.meta.role && auth.user?.role !== to.meta.role) {
    return auth.user?.role === 'sitter' ? '/sitter' : '/owner';
  }

  if (to.meta.guestOnly && isAuthed) {
    return auth.user?.role === 'sitter' ? '/sitter' : '/owner';
  }

  return true;
});

export default router;
