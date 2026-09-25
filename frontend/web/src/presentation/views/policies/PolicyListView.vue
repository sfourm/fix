<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { policyStatusLabel, policyStatusTone } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useToast } from '../../composables/useToast';
import { formatDate, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import PaginationBar from '../../components/PaginationBar.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';

const api = useApi();
const organization = useOrganizationStore();
const router = useRouter();
const toast = useToast();

const page = ref(1);
const { data, loading, error, load } = useLoader(() => api.policies.list({ page: page.value, pageSize: 10 }));

const year = new Date().getFullYear();
const creating = ref(false);
const form = reactive({
  code: `pol-${year}`,
  title: 'Política de gestão de riscos de mercado',
  version: 'v1.0',
  description: '',
  validFrom: `${year}-01-01`,
  validTo: `${year + 1}-12-31`,
  useTemplate: true,
});
const submit = useSubmit();

async function create() {
  const policy = await submit.run(() =>
    api.policies.create({ ...form, description: orNull(form.description), validTo: orNull(form.validTo) }),
  );
  if (policy) {
    toast.success('Política criada em rascunho.');
    router.push(`/policies/${policy.id}`);
  }
}

function goTo(p: number) {
  page.value = p;
  load();
}

onMounted(load);
</script>

<template>
  <PageHeader
    title="Política de riscos"
    subtitle="Política-mãe versionada: limites, eixos por fator de risco, bandas de cobertura e instrumentos. Só vale depois de aprovada em ata."
  >
    <template #actions>
      <button v-if="organization.can(Permission.CreatePolicy)" class="btn btn-primary" @click="creating = true; submit.reset()">+ Nova política</button>
    </template>
  </PageHeader>

  <section class="card">
    <StateBlock :loading="loading && !data" :error="error" :empty="data?.items.length === 0" empty-text="Nenhuma política cadastrada ainda." @retry="load">
      <div class="table-wrap">
        <table class="table">
          <thead>
            <tr><th>Código</th><th>Título</th><th>Versão</th><th>Status</th><th>Vigência</th><th>Eixos</th></tr>
          </thead>
          <tbody>
            <tr v-for="policy in data?.items ?? []" :key="policy.id" class="clickable" @click="router.push(`/policies/${policy.id}`)">
              <td><strong>{{ policy.code.toUpperCase() }}</strong></td>
              <td>{{ policy.title }}</td>
              <td>{{ policy.version }}</td>
              <td><StatusBadge :label="policyStatusLabel[policy.status]" :tone="policyStatusTone[policy.status]" /></td>
              <td class="muted">{{ formatDate(policy.validFrom) }} → {{ formatDate(policy.validTo) }}</td>
              <td>{{ policy.axesCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <PaginationBar v-if="data" :page="data.page" :page-size="data.pageSize" :total-count="data.totalCount" @change="goTo" />
    </StateBlock>
  </section>

  <BaseModal v-if="creating" title="Nova política" @close="creating = false">
    <form id="policy-form" class="stack" @submit.prevent="create">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <div class="form-grid">
        <div class="field">
          <label for="policy-code">Código</label>
          <input id="policy-code" v-model="form.code" class="input" maxlength="32" required />
        </div>
        <div class="field">
          <label for="policy-version">Versão</label>
          <input id="policy-version" v-model="form.version" class="input" maxlength="16" required />
        </div>
      </div>
      <div class="field">
        <label for="policy-title">Título</label>
        <input id="policy-title" v-model="form.title" class="input" maxlength="200" required />
      </div>
      <div class="form-grid">
        <div class="field">
          <label for="policy-from">Vigência de</label>
          <input id="policy-from" v-model="form.validFrom" class="input" type="date" required />
        </div>
        <div class="field">
          <label for="policy-to">até</label>
          <input id="policy-to" v-model="form.validTo" class="input" type="date" />
          <span v-if="submit.fieldError('validTo')" class="field-error">{{ submit.fieldError('validTo') }}</span>
        </div>
      </div>
      <div class="field">
        <label for="policy-description">Descrição</label>
        <textarea id="policy-description" v-model="form.description" class="input" maxlength="2000" />
      </div>
      <label class="row"><input v-model="form.useTemplate" type="checkbox" /> Iniciar com o modelo FIX (eixos, bandas de cobertura e instrumentos)</label>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="creating = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="policy-form" :disabled="submit.submitting.value">Criar rascunho</button>
    </template>
  </BaseModal>
</template>
