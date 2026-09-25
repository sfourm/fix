import { createPinia } from 'pinia';
import { createApp } from 'vue';
import { provideApi } from './application/api-provider';
import { useOrganizationStore } from './application/stores/organization.store';
import { useSessionStore } from './application/stores/session.store';
import { createApi } from './infrastructure/api';
import { HttpClient } from './infrastructure/http/http-client';
import App from './presentation/App.vue';
import { vColumns } from './presentation/directives/columns';
import { installSelectPopover } from './presentation/directives/select-popover';
import { router } from './presentation/router';
// Fontes (empacotadas no build, sem CDN): no Apple o texto usa SF Pro do sistema; nos demais, Inter (a mais próxima).
import '@fontsource-variable/inter/wght.css';
import '@fontsource-variable/jetbrains-mono/wght.css';
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
installSelectPopover();

createApp(App).use(pinia).use(router).directive('columns', vColumns).mount('#app');
