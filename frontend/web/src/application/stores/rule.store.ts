import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { baseRoleLabel, type Rule } from '@/domain/rule';
import { useApi } from '../api-provider';
import { useOrganizationStore } from './organization.store';

/** Rules da organização atual (owner, user e alçadas). Recarregadas ao trocar de organização ou editar alçadas. */
export const useRuleStore = defineStore('rules', () => {
  const rules = ref<Rule[]>([]);
  let loadedFor: string | null = null;
  let loading: Promise<void> | null = null;

  /** Alçadas personalizadas da organização (as únicas atribuíveis a membros e grupos). */
  const alcadas = computed(() => rules.value.filter((r) => !r.isSystem));

  function load(force = false): Promise<void> {
    const organizationId = useOrganizationStore().currentId;
    if (!force && loading && loadedFor === organizationId) return loading;

    loadedFor = organizationId;
    loading = useApi()
      .rules.list()
      .then((list) => {
        rules.value = list;
      })
      .catch((error) => {
        loading = null;
        throw error;
      });

    return loading;
  }

  function nameOf(code: string): string {
    return rules.value.find((r) => r.code === code)?.name ?? baseRoleLabel[code] ?? code;
  }

  return { rules, alcadas, load, nameOf };
});
