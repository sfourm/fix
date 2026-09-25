<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useSessionStore } from '@/application/stores/session.store';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import StateBlock from '../../components/StateBlock.vue';

const organization = useOrganizationStore();
const session = useSessionStore();
const router = useRouter();

const { data: organizations, loading, error, load } = useLoader(() => organization.loadMine());
const name = ref('');
const { submitting, error: createError, run } = useSubmit();
const selecting = ref<string | null>(null);
const selectError = ref<string | null>(null);

async function select(id: string) {
  selecting.value = id;
  selectError.value = null;
  try {
    await organization.select(id);
    router.push('/');
  } catch (e) {
    selectError.value = errorMessage(e);
  } finally {
    selecting.value = null;
  }
}

async function create() {
  const created = await run(() => organization.create(name.value));
  if (created) router.push('/');
}

function logout() {
  session.logout();
  organization.reset();
  router.push('/login');
}

onMounted(load);
</script>

<template>
  <div class="stack">
    <div>
      <h1>Suas organizações</h1>
      <p class="muted" style="margin: 4px 0 0">Olá, {{ session.user?.fullName }}. Escolha onde trabalhar.</p>
    </div>

    <p v-if="selectError" class="alert alert-error">{{ selectError }}</p>

    <StateBlock :loading="loading" :error="error" :empty="organizations?.length === 0" empty-text="Você ainda não participa de nenhuma organização." @retry="load">
      <ul class="orgs">
        <li v-for="org in organizations ?? []" :key="org.id">
          <button class="org-item" type="button" :disabled="selecting !== null" @click="select(org.id)">
            <span class="logo">{{ org.name.charAt(0).toUpperCase() }}</span>
            <span class="stack" style="gap: 0; text-align: left">
              <strong>{{ org.name }}</strong>
              <span class="muted small">{{ org.slug }}</span>
            </span>
            <span v-if="selecting === org.id" class="spinner" style="margin-left: auto" />
          </button>
        </li>
      </ul>
    </StateBlock>

    <form class="stack create" @submit.prevent="create">
      <h2>Criar nova organização</h2>
      <p class="muted small" style="margin: 0">Você será o founder e fará parte do grupo Administradores.</p>
      <p v-if="createError" class="alert alert-error">{{ createError }}</p>
      <div class="row" style="flex-wrap: nowrap">
        <input v-model="name" class="input" placeholder="Nome da organização" maxlength="150" required />
        <button class="btn btn-primary" type="submit" :disabled="submitting">Criar</button>
      </div>
    </form>

    <button class="btn-link btn small" type="button" style="align-self: center" @click="logout">Sair</button>
  </div>
</template>

<style scoped>
.orgs {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.org-item {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
  font: inherit;
  cursor: pointer;
}

.org-item:hover:not(:disabled) {
  border-color: var(--primary);
  background: var(--primary-soft);
}

.logo {
  display: grid;
  place-items: center;
  width: 34px;
  height: 34px;
  border-radius: 9px;
  background: var(--primary);
  color: #fff;
  font-weight: 700;
}

.create {
  padding-top: 16px;
  border-top: 1px solid var(--border);
  gap: 10px;
}
</style>
