<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useRoleStore } from '@/application/stores/role.store';
import { useRuleStore } from '@/application/stores/rule.store';
import { Permission } from '@/domain/permissions';
import { DECISION_ROLES, ROLE_GROUPS, type Rule } from '@/domain/rule';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import StatusBadge from '../../components/StatusBadge.vue';

const api = useApi();
const organization = useOrganizationStore();
const roleStore = useRoleStore();
const ruleStore = useRuleStore();
const toast = useToast();
const { confirm } = useConfirm();

const canEdit = computed(() => organization.can(Permission.EditOrganization) && !organization.isInternalOrganization);
const systemRules = computed(() => ruleStore.rules.filter((r) => r.isSystem));
const myRoles = computed(() => roleStore.roles.map((r) => ({ ...r, granted: organization.roles.has(r.code) })));
const isDecision = (code: string) => (DECISION_ROLES as string[]).includes(code);

// ---------- Criar / editar alçada ----------
const editor = ref<{ rule: Rule | null } | null>(null);
const form = reactive<{ name: string; roleCodes: string[] }>({ name: '', roleCodes: [] });
const submit = useSubmit();

function open(rule: Rule | null) {
  Object.assign(form, { name: rule?.name ?? '', roleCodes: [...(rule?.roles ?? [])] });
  submit.reset();
  editor.value = { rule };
}

/** Ao marcar uma permissão de ação, marca junto a de visualização da mesma área (sem ver, não há como agir). */
function toggle(code: string, checked: boolean) {
  const set = new Set(form.roleCodes);
  if (checked) {
    set.add(code);
    const view = ROLE_GROUPS.find((g) => g.roles.includes(code as never))?.roles.find((r) => r.startsWith('view_'));
    if (view) set.add(view);
  } else {
    set.delete(code);
  }
  form.roleCodes = [...set];
}

async function save() {
  const editing = editor.value?.rule;
  const input = { name: form.name, roleCodes: form.roleCodes };
  const saved = await submit.run(() => (editing ? api.rules.update(editing.id, input) : api.rules.create(input)));
  if (saved) {
    editor.value = null;
    await Promise.all([ruleStore.load(true), organization.refresh()]);
    toast.success(editing ? 'Alçada atualizada: vale na hora para quem a tem.' : 'Alçada criada.');
  }
}

async function remove(rule: Rule) {
  const ok = await confirm({
    title: 'Excluir alçada',
    message:
      rule.usages > 0
        ? `"${rule.name}" está atribuída a ${rule.usages} membro(s)/grupo(s) e será retirada de todos. Continuar?`
        : `Excluir a alçada "${rule.name}"?`,
    confirmLabel: 'Excluir',
    danger: true,
  });
  if (!ok) return;
  try {
    await api.rules.remove(rule.id);
    await Promise.all([ruleStore.load(true), organization.refresh()]);
    toast.success('Alçada excluída.');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(() => {
  Promise.all([roleStore.load(), ruleStore.load(true)]).catch((e) => toast.error(errorMessage(e)));
});
</script>

<template>
  <PageHeader
    title="Rules e alçadas"
    :subtitle="
      organization.isInternalOrganization
        ? 'Papéis internos da equipe FIX: fixos e nunca disponibilizados às organizações clientes.'
        : 'Toda organização tem um owner (acesso total) e usuários (leitura). As alçadas personalizadas somam permissões e são atribuídas a membros e grupos.'
    "
  >
    <template #actions>
      <button v-if="canEdit" class="btn btn-primary" type="button" @click="open(null)">+ Nova alçada</button>
    </template>
  </PageHeader>

  <div class="stack">
    <section v-if="!organization.isInternalOrganization" class="card">
      <header class="card-header">
        <h2>Alçadas da organização</h2>
        <span class="muted small">{{ ruleStore.alcadas.length }} alçada(s)</span>
      </header>
      <div class="table-wrap">
        <table class="table">
          <thead>
            <tr><th>Alçada</th><th>Permissões</th><th>Em uso</th><th /></tr>
          </thead>
          <tbody>
            <tr v-for="rule in ruleStore.alcadas" :key="rule.id">
              <td>
                <strong>{{ rule.name }}</strong>
                <div class="muted small"><code>{{ rule.code }}</code></div>
              </td>
              <td>
                <div class="row">
                  <span v-for="code in rule.roles" :key="code" class="badge" :class="{ 'badge-warning': isDecision(code) }" :title="roleStore.describe(code)">
                    {{ code }}
                  </span>
                </div>
              </td>
              <td>{{ rule.usages }}</td>
              <td class="actions">
                <template v-if="canEdit">
                  <button class="btn btn-sm" type="button" @click="open(rule)">Editar</button>
                  <button class="btn btn-sm btn-danger" type="button" @click="remove(rule)">Excluir</button>
                </template>
              </td>
            </tr>
            <tr v-if="ruleStore.alcadas.length === 0">
              <td colspan="4" class="muted">Nenhuma alçada. Crie a primeira para dar permissões além da leitura.</td>
            </tr>
          </tbody>
        </table>
      </div>
      <p class="card-body muted small" style="margin: 0; padding-top: 12px">
        <span class="badge badge-warning">amarelo</span> = decisão (aprovar, confirmar, alçada de emissão). Além da role, aprovar exige estar acima
        no organograma.
      </p>
    </section>

    <div class="grid-2">
      <section class="card">
        <header class="card-header"><h2>{{ organization.isInternalOrganization ? 'Papéis internos' : 'Papéis fixos' }}</h2></header>
        <div class="table-wrap">
          <table class="table">
            <tbody>
              <tr v-for="rule in systemRules" :key="rule.id">
                <td>
                  <strong>{{ rule.name }}</strong>
                  <div class="muted small">{{ rule.roles.length }} roles · {{ rule.usages }} em uso</div>
                </td>
                <td class="actions"><span class="badge">sistema</span></td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="card">
        <header class="card-header">
          <h2>Suas roles aqui</h2>
          <StatusBadge v-if="organization.internalAccess" label="Suporte FIX" tone="info" />
        </header>
        <div class="table-wrap">
          <table class="table">
            <tbody>
              <tr v-for="role in myRoles" :key="role.code">
                <td>
                  <code>{{ role.code }}</code>
                  <div class="muted small">{{ role.description }}</div>
                </td>
                <td class="actions">
                  <StatusBadge :label="role.granted ? 'Concedida' : 'Sem acesso'" :tone="role.granted ? 'success' : 'neutral'" />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </div>

  <BaseModal v-if="editor" :title="editor.rule ? `Editar alçada ${editor.rule.name}` : 'Nova alçada'" width="720px" @close="editor = null">
    <form id="rule-form" class="stack" @submit.prevent="save">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <div class="field">
        <label for="rule-name">Nome da alçada</label>
        <input id="rule-name" v-model="form.name" class="input" maxlength="128" required placeholder="Ex.: Mesa Sul, Comitê de riscos" />
      </div>
      <div class="role-groups">
        <fieldset v-for="group in ROLE_GROUPS" :key="group.title">
          <legend>{{ group.title }}</legend>
          <label v-for="code in group.roles" :key="code" class="role-option">
            <input type="checkbox" :checked="form.roleCodes.includes(code)" @change="toggle(code, ($event.target as HTMLInputElement).checked)" />
            <span>
              {{ roleStore.describe(code) }}
              <span v-if="isDecision(code)" class="badge badge-warning">decisão</span>
            </span>
          </label>
        </fieldset>
      </div>
      <p class="muted small" style="margin: 0">{{ form.roleCodes.length }} permissão(ões) selecionada(s).</p>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="editor = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="rule-form" :disabled="submit.submitting.value || form.roleCodes.length === 0">Salvar</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.role-groups {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px 20px;
}

fieldset {
  margin: 0;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
}

legend {
  padding: 0 4px;
  font-size: 0.8rem;
  font-weight: 700;
  color: var(--text-muted);
}

.role-option {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  padding: 3px 0;
  font-size: 0.88rem;
}

.role-option input {
  margin-top: 3px;
}

@media (max-width: 720px) {
  .role-groups {
    grid-template-columns: 1fr;
  }
}
</style>
