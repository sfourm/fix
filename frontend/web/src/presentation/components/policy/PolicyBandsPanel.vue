<script setup lang="ts">
import { reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import type { CoverageBand, CoverageBandInput, Policy } from '@/domain/policy';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import { formatPct, orNull } from '../../composables/format';
import BaseModal from '../BaseModal.vue';

const props = defineProps<{ policy: Policy; editable: boolean }>();
const emit = defineEmits<{ updated: [policy: Policy] }>();

const api = useApi();
const toast = useToast();
const { confirm } = useConfirm();

const modal = ref<{ editing: CoverageBand | null } | null>(null);
const form = reactive({ horizon: '', crop: '', minPct: 0, maxPct: 0, note: '' });
const submit = useSubmit();

function open(editing: CoverageBand | null) {
  Object.assign(form, editing ? { ...editing, note: editing.note ?? '' } : { horizon: '', crop: '', minPct: 0, maxPct: 0, note: '' });
  submit.reset();
  modal.value = { editing };
}

async function save() {
  const editing = modal.value?.editing;
  const input: CoverageBandInput = { horizon: form.horizon, crop: form.crop, minPct: form.minPct, maxPct: form.maxPct, note: orNull(form.note) };
  const updated = await submit.run(() =>
    editing ? api.policies.updateBand(props.policy.id, editing.id, input) : api.policies.addBand(props.policy.id, input),
  );
  if (updated) {
    emit('updated', updated);
    modal.value = null;
    toast.success(editing ? 'Banda atualizada.' : 'Banda adicionada.');
  }
}

async function remove(band: CoverageBand) {
  const ok = await confirm({ title: 'Remover banda', message: `Remover a banda da safra ${band.crop}?`, confirmLabel: 'Remover', danger: true });
  if (!ok) return;
  try {
    emit('updated', await api.policies.removeBand(props.policy.id, band.id));
    toast.success('Banda removida.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}
</script>

<template>
  <div>
    <div v-if="editable" class="card-body" style="padding-bottom: 0">
      <button class="btn btn-primary btn-sm" @click="open(null)">+ Nova banda</button>
    </div>
    <div class="table-wrap">
      <table v-columns="'policy-bands'" class="table">
        <thead>
          <tr><th>Horizonte</th><th>Safra</th><th>Mínimo</th><th>Máximo</th><th>Observação</th><th /></tr>
        </thead>
        <tbody>
          <tr v-for="band in policy.bands" :key="band.id">
            <td><strong>{{ band.horizon }}</strong></td>
            <td>{{ band.crop }}</td>
            <td>{{ formatPct(band.minPct) }}</td>
            <td>{{ formatPct(band.maxPct) }}</td>
            <td class="muted">{{ band.note ?? '—' }}</td>
            <td class="actions">
              <template v-if="editable">
                <button class="btn btn-sm" @click="open(band)">Editar</button>
                <button class="btn btn-sm btn-danger" @click="remove(band)">Remover</button>
              </template>
            </td>
          </tr>
          <tr v-if="policy.bands.length === 0">
            <td colspan="6" class="muted">Nenhuma banda de cobertura.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <BaseModal v-if="modal" :title="modal.editing ? 'Editar banda de cobertura' : 'Nova banda de cobertura'" @close="modal = null">
      <form id="band-form" class="stack" @submit.prevent="save">
        <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
        <div class="form-grid">
          <div class="field">
            <label for="band-horizon">Horizonte</label>
            <input id="band-horizon" v-model="form.horizon" class="input" placeholder="Safra corrente" maxlength="64" required />
          </div>
          <div class="field">
            <label for="band-crop">Safra (AA/AA)</label>
            <input id="band-crop" v-model="form.crop" class="input" placeholder="26/27" maxlength="5" required />
            <span v-if="submit.fieldError('crop')" class="field-error">{{ submit.fieldError('crop') }}</span>
          </div>
          <div class="field">
            <label for="band-min">Cobertura mínima (%)</label>
            <input id="band-min" v-model.number="form.minPct" class="input" type="number" step="any" min="0" max="100" required />
          </div>
          <div class="field">
            <label for="band-max">Cobertura máxima (%)</label>
            <input id="band-max" v-model.number="form.maxPct" class="input" type="number" step="any" min="0" max="100" required />
            <span v-if="submit.fieldError('maxPct')" class="field-error">{{ submit.fieldError('maxPct') }}</span>
          </div>
        </div>
        <div class="field">
          <label for="band-note">Observação</label>
          <input id="band-note" v-model="form.note" class="input" maxlength="500" />
        </div>
      </form>
      <template #footer>
        <button class="btn" type="button" @click="modal = null">Cancelar</button>
        <button class="btn btn-primary" type="submit" form="band-form" :disabled="submit.submitting.value">Salvar</button>
      </template>
    </BaseModal>
  </div>
</template>
