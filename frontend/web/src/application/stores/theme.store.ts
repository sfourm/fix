import { defineStore } from 'pinia';
import { computed, ref, watch } from 'vue';
import { storage } from '@/infrastructure/storage/local-storage';

export type ThemeMode = 'light' | 'dark' | 'system';

const STORAGE_KEY = 'fix.theme';
const media = typeof window !== 'undefined' ? window.matchMedia('(prefers-color-scheme: dark)') : null;

/**
 * Tema do portal: claro, escuro ou automático (segue o sistema). A escolha fica no navegador e o tema resolvido vai
 * para <html data-theme>, que o CSS usa. O index.html aplica o mesmo antes da primeira pintura.
 */
export const useThemeStore = defineStore('theme', () => {
  const mode = ref<ThemeMode>(storage.get<ThemeMode>(STORAGE_KEY) ?? 'system');
  const systemDark = ref(media?.matches ?? false);
  const resolved = computed<'light' | 'dark'>(() => (mode.value === 'system' ? (systemDark.value ? 'dark' : 'light') : mode.value));

  media?.addEventListener('change', (e) => (systemDark.value = e.matches));

  watch(
    resolved,
    (theme) => {
      document.documentElement.dataset.theme = theme;
      document.querySelectorAll('meta[name="theme-color"]').forEach((m) => m.setAttribute('content', theme === 'dark' ? '#000000' : '#f5f5f7'));
    },
    { immediate: true },
  );

  function set(next: ThemeMode) {
    mode.value = next;
    storage.set(STORAGE_KEY, next);
  }

  return { mode, resolved, set };
});
