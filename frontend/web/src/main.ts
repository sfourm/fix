import { createPinia } from 'pinia';
import { createApp } from 'vue';
import { provideApi } from './application/api-provider';
import { useOrganizationStore } from './application/stores/organization.store';
import { useSessionStore } from './application/stores/session.store';
import { createApi } from './infrastructure/api';
import { HttpClient } from './infrastructure/http/http-client';
import App from './presentation/App.vue';
import { router } from './presentation/router';
import './presentation/styles/main.css';

// Composition root: a infraestrutura HTTP lê token e tenant das stores da aplicação.
const pinia = createPinia();

const http = new HttpClient({
  baseUrl: import.meta.env.VITE_BFF_URL ?? '/api',
  getToken: () => useSessionStore(pinia).token,
  getOrganizationId: () => useOrganizationStore(pinia).currentId,
  onUnauthorized: async () => {
    const redirect = router.currentRoute.value.fullPath;
    useSessionStore(pinia).logout();
    await router.push({ path: '/login', query: { redirect } });
    useOrganizationStore(pinia).reset();
  },
});

provideApi(createApi(http));

createApp(App).use(pinia).use(router).mount('#app');
