import type { Api } from '@/infrastructure/api';

let instance: Api | null = null;

/** Registrado uma única vez no composition root (main.ts). */
export function provideApi(api: Api): void {
  instance = api;
}

export function useApi(): Api {
  if (!instance) {
    throw new Error('API não inicializada. Chame provideApi() no bootstrap.');
  }

  return instance;
}
