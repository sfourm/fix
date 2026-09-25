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

const api = useApi();
const organization = useOrganizationStore();
const roleStore = useRoleStore();
const ruleStore = useRuleStore();
const toast = useToast();
const { confirm } = useConfirm();

const canEdit = computed(() => organization.can(Permission.EditOrganization) && !organization.isInternalOrganization);
const isDecision = (code: string) => (DECISION_ROLES as string[]).includes(code);

// ---------- Criar / editar cargo ----------
const editor = ref<{ rule: Rule | null } | null>(null);
const form = reactive<{ name: string; roleCodes: string[] }>({ name: '', roleCodes: [] });
const submit = useSubmit();

function open(rule: Rule | null) {
  Object.assign(form, { name: rule?.name ?? '', roleCodes: [...(rule?.roles ?? [])] });
  submit.reset();
  editor.value = { rule };
}

/** Ao marcar uma regra de ação, marca junto a de visualização da mesma área (sem ver, não há como agir). */
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
    toast.success(editing ? 'Cargo atualizado: vale na hora para quem o tem.' : 'Cargo criado.');
  }
}

async function remove(rule: Rule) {
  const ok = await confirm({
    title: 'Excluir cargo',
    message:
      rule.usages > 0
        ? `"${rule.name}" está atribuído a ${rule.usages} membro(s)/grupo(s) e será retirado de todos. Continuar?`
        : `Excluir o cargo "${rule.name}"?`,
    confirmLabel: 'Excluir',
    danger: true,
  });
  if (!ok) return;
  try {
    await api.rules.remove(rule.id);
    await Promise.all([ruleStore.load(true), organization.refresh()]);
    toast.success('Cargo excluído.');
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
    title="Cargos e Regras"
    :subtitle="
      organization.isInternalOrganization
        ? 'A equipe FIX usa papéis internos fixos: não há cargos para configurar aqui.'
        : 'Cada cargo reúne regras de acesso e é atribuído a membros e grupos. Sem cargo, o membro só tem leitura; o owner tem acesso total.'
    "
  >
    <template #actions>
      <button v-if="canEdit" class="btn btn-primary" type="button" @click="open(null)">+ Novo cargo</button>
    </template>
  </PageHeader>

  <section v-if="!organization.isInternalOrganization" class="card">
    <header class="card-header">
      <h2>Cargos da organização</h2>
      <span class="muted small">{{ ruleStore.alcadas.length }} cargo(s)</span>
    </header>
    <div class="table-wrap">
      <table v-columns="'cargos'" class="table">
        <thead>
          <tr><th>Cargo</th><th>Regras</th><th>Em uso</th><th /></tr>
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
            <td colspan="4" class="muted">Nenhum cargo. Crie o primeiro para dar regras além da leitura.</td>
          </tr>
        </tbody>
      </table>
    </div>
    <p class="card-body muted small" style="margin: 0; padding-top: 12px">
      <span class="badge badge-warning">amarelo</span> = decisão (aprovar, confirmar, alçada de emissão). Além da regra, aprovar exige estar acima
      no organograma.
    </p>
  </section>

  <BaseModal v-if="editor" :title="editor.rule ? `Editar cargo ${editor.rule.name}` : 'Novo cargo'" width="720px" @close="editor = null">
    <form id="rule-form" class="stack" @submit.prevent="save">
      <p v-if="submit.error.value" class="alert alert-error">{{ submit.error.value }}</p>
      <div class="field">
        <label for="rule-name">Nome do cargo</label>
        <input id="rule-name" v-model="form.name" class="input" maxlength="128" required placeholder="Ex.: Trader, Gerente de risco" />
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
      <p class="muted small" style="margin: 0">{{ form.roleCodes.length }} regra(s) selecionada(s).</p>
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
  padding: 12px 14px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
}

legend {
  padding: 0 4px;
  font-size: 0.8rem;
  font-weight: 650;
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
  margin-top: 0;
}

@media (max-width: 720px) {
  .role-groups {
    grid-template-columns: 1fr;
  }
}
</style>
