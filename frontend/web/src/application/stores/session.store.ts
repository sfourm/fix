import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import type { Session } from '@/domain/session';
import { storage } from '@/infrastructure/storage/local-storage';
import { useApi } from '../api-provider';

const STORAGE_KEY = 'fix.session';

const isValid = (session: Session | null): session is Session =>
  !!session && new Date(session.expiresAt).getTime() > Date.now();

export const useSessionStore = defineStore('session', () => {
  const stored = storage.get<Session>(STORAGE_KEY);
  const session = ref<Session | null>(isValid(stored) ? stored : null);

  const token = computed(() => (isValid(session.value) ? session.value.token : null));
  const user = computed(() => session.value?.user ?? null);
  const isAuthenticated = computed(() => token.value !== null);
  /** Papel na equipe interna FIX (super_administrador, administrador) ou null para usuários de clientes. */
  const internalRole = computed(() => user.value?.roles.find((r) => r === 'super_administrador' || r === 'administrador') ?? null);

  function start(value: Session) {
    session.value = value;
    storage.set(STORAGE_KEY, value);
  }

  async function login(input: { email: string; password: string }) {
    start(await useApi().auth.login(input));
  }

  async function register(input: { email: string; password: string; fullName: string }) {
    start(await useApi().auth.register(input));
  }

  function logout() {
    session.value = null;
    storage.remove(STORAGE_KEY);
  }

  return { session, token, user, isAuthenticated, internalRole, login, register, logout };
});
