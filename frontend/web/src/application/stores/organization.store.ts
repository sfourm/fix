import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import type { Organization, OrganizationSetup } from '@/domain/organization';
import type { PermissionCode } from '@/domain/permissions';
import { storage } from '@/infrastructure/storage/local-storage';
import { useApi } from '../api-provider';

const STORAGE_KEY = 'fix.organizationId';

/** Organização (tenant) selecionada, seu setup e as roles efetivas (alçadas) do usuário nela. */
export const useOrganizationStore = defineStore('organization', () => {
  const currentId = ref<string | null>(storage.get<string>(STORAGE_KEY));
  const current = ref<OrganizationSetup | null>(null);
  const organizations = ref<Organization[]>([]);
  const roles = ref<Set<string>>(new Set());

  const hasOrganization = computed(() => currentId.value !== null);
  /** Como o usuário acessa a organização atual: membro ou suporte interno FIX (vê e edita, não decide). */
  const currentEntry = computed(() => organizations.value.find((o) => o.id === currentId.value) ?? null);
  const internalAccess = computed(() => currentEntry.value?.internalAccess ?? false);
  const isInternalOrganization = computed(() => currentEntry.value?.isInternal ?? false);

  function can(role: PermissionCode): boolean {
    return roles.value.has(role);
  }

  async function loadMine() {
    organizations.value = await useApi().organizations.listMine();
    return organizations.value;
  }

  async function select(organizationId: string) {
    currentId.value = organizationId;
    storage.set(STORAGE_KEY, organizationId);
    try {
      await refresh();
    } catch (error) {
      clear();
      throw error;
    }
  }

  /** Recarrega setup e roles (ex.: após trocar de tenant ou alterar rules de membros). */
  async function refresh() {
    const api = useApi();
    const [setup, userRoles] = await Promise.all([
      api.organizations.setup(),
      api.organizations.roles(),
      organizations.value.length ? Promise.resolve() : loadMine(),
    ]);
    current.value = setup;
    roles.value = new Set(userRoles);
  }

  /** Aplica o setup devolvido pelas operações de edição (evita um GET extra). */
  function applySetup(setup: OrganizationSetup) {
    current.value = setup;
    organizations.value = organizations.value.map((o) => (o.id === setup.id ? { ...o, name: setup.name, slug: setup.slug } : o));
  }

  async function create(name: string) {
    const organization = await useApi().organizations.create(name);
    organizations.value = [...organizations.value, organization];
    await select(organization.id);
    return organization;
  }

  async function rename(name: string) {
    const renamed = await useApi().organizations.rename(name);
    if (current.value) applySetup({ ...current.value, name: renamed.name, slug: renamed.slug });
  }

  function clear() {
    currentId.value = null;
    current.value = null;
    roles.value = new Set();
    storage.remove(STORAGE_KEY);
  }

  function reset() {
    clear();
    organizations.value = [];
  }

  return {
    currentId,
    current,
    organizations,
    roles,
    hasOrganization,
    internalAccess,
    isInternalOrganization,
    can,
    loadMine,
    select,
    refresh,
    applySetup,
    create,
    rename,
    clear,
    reset,
  };
});
