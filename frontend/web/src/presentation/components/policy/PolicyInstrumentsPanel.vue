<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { instrumentPermissionLabel, instrumentPermissionTone } from '@/domain/labels';
import { INSTRUMENT_PERMISSIONS, type Policy, type PolicyInstrument, type PolicyInstrumentInput } from '@/domain/policy';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { orNull } from '../../composables/format';
import BaseModal from '../BaseModal.vue';
import StatusBadge from '../StatusBadge.vue';

const props = defineProps<{ policy: Policy; editable: boolean }>();
const emit = defineEmits<{ updated: [policy: Policy] }>();

const api = useApi();
const toast = useToast();
const { confirm } = useConfirm();

const modal = ref<{ editing: PolicyInstrument | null } | null>(null);
const form = reactive({ name: '', permission: 'Allowed' as PolicyInstrument['permission'], condition: '' });
const submit = useSubmit();

function open(editing: PolicyInstrument | null) {
  Object.assign(form, { name: editing?.name ?? '', permission: editing?.permission ?? 'Allowed', condition: editing?.condition ?? '' });
  submit.reset();
  modal.value = { editing };
}

async function save() {
  const editing = modal.value?.editing;
  const input: PolicyInstrumentInput = { name: form.name, permission: form.permission, condition: orNull(form.condition) };
  const updated = await submit.run(() =>
    editing
      ? api.policies.updateInstrument(props.policy.id, editing.id, input)
      : api.policies.addInstrument(props.policy.id, input),
  );
  if (updated) {
    emit('updated', updated);
    modal.value = null;
    toast.success(editing ? 'Instrumento atualizado.' : 'Instrumento adicionado.');
  }
}

async function remove(instrument: PolicyInstrument) {
  const ok = await confirm({ title: 'Remover instrumento', message: `Remover ${instrument.name}?`, confirmLabel: 'Remover', danger: true });
  if (!ok) return;
  try {
    emit('updated', await api.policies.removeInstrument(props.policy.id, instrument.id));
    toast.success('Instrumento removido.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}
</script>

<template>
  <div>
    <div v-if="editable" class="card-body" style="padding-bottom: 0">
      <button class="btn btn-primary btn-sm" @click="open(null)">+ Novo instrumento</button>
    </div>
    <div class="table-wrap">
      <table class="table">
        <thead>
          <tr><th>Instrumento</th><th>Permissão</th><th>Condição</th><th /></tr>
        </thead>
        <tbody>
          <tr v-for="instrument in policy.instruments" :key="instrument.id">
            <td><strong>{{ instrument.name }}</strong></td>
            <td>
              <StatusBadge :label="instrumentPermissionLabel[instrument.permission]" :tone="instrumentPermissionTone[instrument.permission]" />
            </td>
            <td class="muted">{{ instrument.condition ?? '—' }}</td>
            <td class="actions">
              <template v-if="editable">
                <button class="btn btn-sm" @click="open(instrument)">Editar</button>
                <button class="btn btn-sm btn-danger" @click="remove(instrument)">Remover</button>
              </template>
            </td>
          </tr>
          <tr v-if="policy.instruments.length === 0">
            <td colspan="4" class="muted">Nenhum instrumento cadastrado.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <BaseModal v-if="modal" :title="modal.editing ? 'Editar instrumento' : 'Novo instrumento'" @close="modal = null">
      <form id="instrument-form" class="stack" @submit.prevent="save">
        <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
        <div class="field">
          <label for="instrument-name">Instrumento</label>
          <input id="instrument-name" v-model="form.name" class="input" maxlength="200" required />
        </div>
        <div class="field">
          <label for="instrument-permission">Permissão</label>
          <select id="instrument-permission" v-model="form.permission" class="input">
            <option v-for="p in INSTRUMENT_PERMISSIONS" :key="p" :value="p">{{ instrumentPermissionLabel[p] }}</option>
          </select>
        </div>
        <div class="field">
          <label for="instrument-condition">Condição</label>
          <input id="instrument-condition" v-model="form.condition" class="input" maxlength="500" />
        </div>
      </form>
      <template #footer>
        <button class="btn" type="button" @click="modal = null">Cancelar</button>
        <button class="btn btn-primary" type="submit" form="instrument-form" :disabled="submit.submitting.value">Salvar</button>
      </template>
    </BaseModal>
  </div>
</template>
