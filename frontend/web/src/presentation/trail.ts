import { defineStore } from 'pinia';
import { reactive } from 'vue';
import type { RouteLocationNormalizedLoaded } from 'vue-router';

/** Um passo do caminho mostrado na barra superior (breadcrumb) e no menu lateral. */
export interface Crumb {
  label: string;
  to?: string;
}

/**
 * Nomes das entidades abertas (política, mandato, boleta, grupo), registrados pelas telas quando os dados chegam.
 * As rotas montam o breadcrumb com eles; enquanto não chegam, o passo aparece como "…".
 */
export const useTrailStore = defineStore('trail', () => {
  const labels = reactive<Record<string, string>>({});

  function set(id: string, label: string) {
    labels[id] = label;
  }

  function get(id: string | undefined, fallback = '…'): string {
    return (id && labels[id]) || fallback;
  }

  return { labels, set, get };
});

export type TrailStore = ReturnType<typeof useTrailStore>;

declare module 'vue-router' {
  interface RouteMeta {
    /** Caminho da tela (do mais geral para o atual). */
    trail?: (params: Record<string, string>, trail: TrailStore) => Crumb[];
  }
}

/** Breadcrumb da rota atual: usa a definição mais específica entre as rotas casadas. */
export function crumbsFor(route: RouteLocationNormalizedLoaded, trail: TrailStore): Crumb[] {
  const record = [...route.matched].reverse().find((r) => r.meta.trail);
  return record?.meta.trail ? record.meta.trail(route.params as Record<string, string>, trail) : [];
}
