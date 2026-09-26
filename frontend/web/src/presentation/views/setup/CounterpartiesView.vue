<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { COUNTERPARTY_TYPES, type Counterparty, type CounterpartyInput } from '@/domain/counterparty';
import { counterpartyTypeLabel } from '@/domain/labels';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatUsd, orNull } from '../../composables/format';
import BaseModal from '../../components/BaseModal.vue';
import NumberInput from '../../components/NumberInput.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import StatusBadge from '../../components/StatusBadge.vue';

const api = useApi();
const organization = useOrganizationStore();
const toast = useToast();
const { confirm } = useConfirm();

const onlyHomologated = ref(false);
const { data, loading, error, load } = useLoader(() => api.counterparties.list(onlyHomologated.value));
const canManage = computed(() => organization.can(Permission.ManageCounterparties));

const empty = (): CounterpartyInput => ({
  name: '',
  type: 'Trading',
  document: null,
  address: null,
  country: null,
  notionalLimitUsd: null,
  mtmLimitUsd: null,
});

const modal = ref<{ editing: Counterparty | null } | null>(null);
const form = reactive<CounterpartyInput>(empty());
const submit = useSubmit();

function open(editing: Counterparty | null) {
  const { id: _, isHomologated: __, ...input } = editing ?? { ...empty(), id: '', isHomologated: true };
  Object.assign(form, input);
  submit.reset();
  modal.value = { editing };
}

async function save() {
  const editing = modal.value?.editing;
  const input: CounterpartyInput = {
    ...form,
    document: orNull(form.document),
    address: orNull(form.address),
    country: orNull(form.country),
  };
  const saved = await submit.run(() => (editing ? api.counterparties.update(editing.id, input) : api.counterparties.create(input)));
  if (saved) {
    modal.value = null;
    toast.success(editing ? 'Contraparte atualizada.' : 'Contraparte cadastrada (homologada).');
    load();
  }
}

async function toggleHomologation(counterparty: Counterparty) {
  try {
    await api.counterparties.setHomologation(counterparty.id, !counterparty.isHomologated);
    toast.success(counterparty.isHomologated ? 'Homologação suspensa: a contraparte não aceita novas boletas.' : 'Contraparte homologada.');
    load();
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

async function remove(counterparty: Counterparty) {
  const ok = await confirm({
    title: 'Excluir contraparte',
    message: `Excluir "${counterparty.name}"? Contrapartes com boletas não podem ser excluídas — suspenda a homologação.`,
    confirmLabel: 'Excluir',
    danger: true,
  });
  if (!ok) return;
  try {
    await api.counterparties.remove(counterparty.id);
    toast.success('Contraparte excluída.');
    load();
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(load);
</script>

<template>
  <PageHeader title="Contrapartes" subtitle="Cadastro de apoio: só contrapartes homologadas aparecem nas boletas (§5.5).">
    <template #actions>
      <label class="row small"><input v-model="onlyHomologated" type="checkbox" @change="load" /> Somente homologadas</label>
      <button v-if="canManage" class="btn btn-primary" @click="open(null)">+ Nova contraparte</button>
    </template>
  </PageHeader>

  <section class="card">
    <StateBlock :loading="loading && !data" :error="error" :empty="data?.length === 0" empty-text="Nenhuma contraparte cadastrada." @retry="load">
      <div class="table-wrap">
        <table v-columns="'counterparties'" class="table">
          <thead>
            <tr><th>Código</th><th>Contraparte</th><th>Tipo</th><th>País</th><th>Limite nocional</th><th>Limite MtM</th><th>Situação</th><th /></tr>
          </thead>
          <tbody>
            <tr v-for="c in data ?? []" :key="c.id">
              <td class="num muted">{{ c.code }}</td>
              <td>
                <strong>{{ c.name }}</strong>
                <div v-if="c.document" class="muted small">{{ c.document }}</div>
              </td>
              <td>{{ counterpartyTypeLabel[c.type] }}</td>
              <td>{{ c.country ?? '—' }}</td>
              <td>{{ formatUsd(c.notionalLimitUsd) }}</td>
              <td>{{ formatUsd(c.mtmLimitUsd) }}</td>
              <td><StatusBadge :label="c.isHomologated ? 'Homologada' : 'Suspensa'" :tone="c.isHomologated ? 'success' : 'danger'" /></td>
              <td class="actions">
                <template v-if="canManage">
                  <button class="btn btn-sm" @click="toggleHomologation(c)">{{ c.isHomologated ? 'Suspender' : 'Homologar' }}</button>
                  <button class="btn btn-sm" @click="open(c)">Editar</button>
                  <button class="btn btn-sm btn-danger" @click="remove(c)">Excluir</button>
                </template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </StateBlock>
  </section>

  <BaseModal v-if="modal" :title="modal.editing ? 'Editar contraparte' : 'Nova contraparte'" @close="modal = null">
    <form id="counterparty-form" class="stack" @submit.prevent="save">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <div class="field">
        <label for="cp-name">Nome</label>
        <input id="cp-name" v-model="form.name" class="input" maxlength="200" required autofocus />
        <span v-if="submit.fieldError('name')" class="field-error">{{ submit.fieldError('name') }}</span>
      </div>
      <div class="form-grid">
        <div class="field">
          <label for="cp-type">Tipo</label>
          <select id="cp-type" v-model="form.type" class="input">
            <option v-for="t in COUNTERPARTY_TYPES" :key="t" :value="t">{{ counterpartyTypeLabel[t] }}</option>
          </select>
        </div>
        <div class="field">
          <label for="cp-document">Documento</label>
          <input id="cp-document" v-model="form.document" class="input" maxlength="32" />
        </div>
        <div class="field">
          <label for="cp-country">País</label>
          <input id="cp-country" v-model="form.country" class="input" maxlength="64" />
        </div>
        <div class="field">
          <label for="cp-address">Endereço</label>
          <input id="cp-address" v-model="form.address" class="input" maxlength="300" />
        </div>
        <div class="field">
          <label for="cp-notional">Limite nocional (US$)</label>
          <NumberInput id="cp-notional" v-model="form.notionalLimitUsd" :min="0" />
        </div>
        <div class="field">
          <label for="cp-mtm">Limite MtM (US$)</label>
          <NumberInput id="cp-mtm" v-model="form.mtmLimitUsd" :min="0" />
        </div>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="modal = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="counterparty-form" :disabled="submit.submitting.value">Salvar</button>
    </template>
  </BaseModal>
</template>
