import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import { isAssignableRule, type Rule } from '@/domain/rule';
import { useApi } from '../api-provider';

/** Rules do sistema (conjuntos de roles, fixas, carregadas uma vez). */
export const useRuleStore = defineStore('rules', () => {
  const rules = ref<Rule[]>([]);
  let loading: Promise<void> | null = null;

  /** Rules que podem ser atribuídas dentro de uma organização. */
  const assignable = computed(() => rules.value.filter(isAssignableRule));

  function load(): Promise<void> {
    loading ??= useApi()
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
    return rules.value.find((r) => r.code === code)?.name ?? code;
  }

  return { rules, assignable, load, nameOf };
});
