<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { riskFactorLabel } from '@/domain/labels';
import { RISK_FACTORS, type Policy, type PolicyAxis, type PolicyAxisInput } from '@/domain/policy';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { orNull } from '../../composables/format';
import BaseModal from '../BaseModal.vue';

const props = defineProps<{ policy: Policy; editable: boolean }>();
const emit = defineEmits<{ updated: [policy: Policy] }>();

const api = useApi();
const toast = useToast();
const { confirm } = useConfirm();

const modal = ref<{ editing: PolicyAxis | null } | null>(null);
const form = reactive({ code: '', title: '', factor: 'Price' as PolicyAxis['factor'], statement: '', limitDescription: '', approver: '', restrictions: '' });
const submit = useSubmit();

function open(editing: PolicyAxis | null) {
  Object.assign(form, {
    code: editing?.code ?? '',
    title: editing?.title ?? '',
    factor: editing?.factor ?? 'Price',
    statement: editing?.statement ?? '',
    limitDescription: editing?.limitDescription ?? '',
    approver: editing?.approver ?? '',
    restrictions: editing?.restrictions.join('\n') ?? '',
  });
  submit.reset();
  modal.value = { editing };
}

async function save() {
  const editing = modal.value?.editing;
  const input: PolicyAxisInput = {
    code: form.code,
    title: form.title,
    factor: form.factor,
    statement: orNull(form.statement),
    limitDescription: orNull(form.limitDescription),
    approver: orNull(form.approver),
    restrictions: form.restrictions.split('\n').map((r) => r.trim()).filter(Boolean),
  };
  const updated = await submit.run(() =>
    editing ? api.policies.updateAxis(props.policy.id, editing.id, input) : api.policies.addAxis(props.policy.id, input),
  );
  if (updated) {
    emit('updated', updated);
    modal.value = null;
    toast.success(editing ? 'Eixo atualizado.' : 'Eixo adicionado.');
  }
}

async function remove(axis: PolicyAxis) {
  const ok = await confirm({ title: 'Remover eixo', message: `Remover o eixo ${axis.code} · ${axis.title}?`, confirmLabel: 'Remover', danger: true });
  if (!ok) return;
  try {
    emit('updated', await api.policies.removeAxis(props.policy.id, axis.id));
    toast.success('Eixo removido.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}
</script>

<template>
  <div>
    <div v-if="editable" class="card-body" style="padding-bottom: 0">
      <button class="btn btn-primary btn-sm" @click="open(null)">+ Novo eixo</button>
    </div>
    <div class="axes">
      <article v-for="axis in policy.axes" :key="axis.id" class="axis">
        <header class="row" style="justify-content: space-between">
          <div class="row">
            <span class="badge badge-primary">{{ axis.code }}</span>
            <strong>{{ axis.title }}</strong>
            <span class="badge">{{ riskFactorLabel[axis.factor] }}</span>
          </div>
          <div v-if="editable" class="row">
            <button class="btn btn-sm" @click="open(axis)">Editar</button>
            <button class="btn btn-sm btn-danger" @click="remove(axis)">Remover</button>
          </div>
        </header>
        <p v-if="axis.statement" class="small" style="margin: 8px 0 0">{{ axis.statement }}</p>
        <dl class="details small" style="margin-top: 8px">
          <dt>Limite</dt>
          <dd>{{ axis.limitDescription ?? '—' }}</dd>
          <dt>Aprovador</dt>
          <dd>{{ axis.approver ?? '—' }}</dd>
        </dl>
        <ul v-if="axis.restrictions.length" class="small muted restrictions">
          <li v-for="r in axis.restrictions" :key="r">{{ r }}</li>
        </ul>
      </article>
      <p v-if="policy.axes.length === 0" class="muted">Nenhum eixo cadastrado. Mandatos precisam de um eixo do fator de risco correspondente.</p>
    </div>

    <BaseModal v-if="modal" :title="modal.editing ? 'Editar eixo' : 'Novo eixo'" width="640px" @close="modal = null">
      <form id="axis-form" class="stack" @submit.prevent="save">
        <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
        <div class="form-grid">
          <div class="field">
            <label for="axis-code">Código</label>
            <input id="axis-code" v-model="form.code" class="input" maxlength="16" required />
          </div>
          <div class="field">
            <label for="axis-factor">Fator de risco</label>
            <select id="axis-factor" v-model="form.factor" class="input">
              <option v-for="f in RISK_FACTORS" :key="f" :value="f">{{ riskFactorLabel[f] }}</option>
            </select>
          </div>
        </div>
        <div class="field">
          <label for="axis-title">Título</label>
          <input id="axis-title" v-model="form.title" class="input" maxlength="200" required />
        </div>
        <div class="field">
          <label for="axis-statement">Diretriz</label>
          <textarea id="axis-statement" v-model="form.statement" class="input" maxlength="2000" />
        </div>
        <div class="form-grid">
          <div class="field">
            <label for="axis-limit">Limite</label>
            <input id="axis-limit" v-model="form.limitDescription" class="input" maxlength="1000" />
          </div>
          <div class="field">
            <label for="axis-approver">Aprovador</label>
            <input id="axis-approver" v-model="form.approver" class="input" maxlength="200" />
          </div>
        </div>
        <div class="field">
          <label for="axis-restrictions">Restrições (uma por linha)</label>
          <textarea id="axis-restrictions" v-model="form.restrictions" class="input" />
        </div>
      </form>
      <template #footer>
        <button class="btn" type="button" @click="modal = null">Cancelar</button>
        <button class="btn btn-primary" type="submit" form="axis-form" :disabled="submit.submitting.value">Salvar</button>
      </template>
    </BaseModal>
  </div>
</template>

<style scoped>
.axes {
  display: grid;
  gap: 12px;
  padding: 20px;
}

.axis {
  padding: 14px 16px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
}

.restrictions {
  margin: 8px 0 0;
  padding-left: 18px;
}
</style>
