import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { DataSource, DataSourceField, DataSourceInfo } from '@/domain/search';
import { useApi } from '../api-provider';
import { useOrganizationStore } from './organization.store';

/** Catálogo de conjuntos de dados pesquisáveis (campos, operadores, opções), carregado uma vez por sessão. */
export const useCatalogStore = defineStore('catalog', () => {
  const sources = ref<DataSourceInfo[]>([]);
  let loading: Promise<void> | null = null;

  function load(): Promise<void> {
    loading ??= useApi()
      .search.sources()
      .then((list) => {
        sources.value = list;
      })
      .catch((error) => {
        loading = null;
        throw error;
      });

    return loading;
  }

  const source = (key: DataSource) => sources.value.find((s) => s.key === key);
  const field = (key: DataSource, fieldKey: string | null): DataSourceField | undefined =>
    fieldKey ? source(key)?.fields.find((f) => f.key === fieldKey) : undefined;

  /** Conjuntos que o usuário pode ler na organização atual. */
  const readable = () => {
    const organization = useOrganizationStore();
    return sources.value.filter((s) => organization.roles.has(s.role));
  };

  return { sources, load, source, field, readable };
});
