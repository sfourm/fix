import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { Role } from '@/domain/role';
import { useApi } from '../api-provider';

/** Roles do sistema (permissões atômicas, fixas, carregadas uma vez). */
export const useRoleStore = defineStore('roles', () => {
  const roles = ref<Role[]>([]);
  let loading: Promise<void> | null = null;

  function load(): Promise<void> {
    loading ??= useApi()
      .roles.list()
      .then((list) => {
        roles.value = list;
      })
      .catch((error) => {
        loading = null;
        throw error;
      });

    return loading;
  }

  function describe(code: string): string {
    return roles.value.find((r) => r.code === code)?.description ?? code;
  }

  return { roles, load, describe };
});
