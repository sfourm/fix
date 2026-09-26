<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useRoleStore } from '@/application/stores/role.store';
import { useRuleStore } from '@/application/stores/rule.store';
import type { Group, Member } from '@/domain/organization';
import { Permission } from '@/domain/permissions';
import { DECISION_ROLES, ROLE_GROUPS } from '@/domain/rule';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import { paths } from '../../paths';
import { useTrailStore } from '../../trail';

/**
 * Página do grupo: alçadas atribuídas (com as roles que dão), membros, regras efetivas resultantes e a posição
 * no organograma. Tudo que o grupo concede vale para todos os membros dele.
 */
const props = defineProps<{ groupId: string }>();

const api = useApi();
const organization = useOrganizationStore();
const ruleStore = useRuleStore();
const roleStore = useRoleStore();
const trail = useTrailStore();
const router = useRouter();
const toast = useToast();
const { confirm } = useConfirm();

const groups = ref<Group[]>([]);
const members = ref<Member[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);
const busy = ref(false);

const group = computed(() => groups.value.find((g) => g.id === props.groupId) ?? null);
const canEdit = computed(() => organization.can(Permission.EditOrganization) && !organization.isInternalOrganization);
const isRoot = computed(() => group.value?.parentGroupId === null);

async function load() {
  loading.value = true;
  error.value = null;
  try {
    [groups.value, members.value] = await Promise.all([api.organizations.groups(), api.organizations.members(), ruleStore.load(), roleStore.load()]);
    if (group.value) trail.set(group.value.id, group.value.name);
  } catch (e) {
    error.value = errorMessage(e);
  } finally {
    loading.value = false;
  }
}

/** Aplica o resultado (a API devolve a lista de grupos atualizada) e avisa. */
function applied(updated: Group[], message: string) {
  groups.value = updated;
  if (group.value) trail.set(group.value.id, group.value.name);
  organization.refresh().catch(() => undefined);
  toast.success(message);
}

async function run(action: () => Promise<Group[]>, message: string) {
  busy.value = true;
  try {
    applied(await action(), message);
  } catch (e) {
    toast.error(errorMessage(e));
  } finally {
    busy.value = false;
  }
}

// ---------- Organograma ----------
const byId = computed(() => new Map(groups.value.map((g) => [g.id, g])));
const path = computed(() => {
  const chain: Group[] = [];
  let current = group.value;
  while (current) {
    chain.unshift(current);
    current = current.parentGroupId ? (byId.value.get(current.parentGroupId) ?? null) : null;
  }
  return chain;
});
const children = computed(() => groups.value.filter((g) => g.parentGroupId === props.groupId));
/** Grupos que podem ficar acima: todos menos ele mesmo e os que estão abaixo dele. */
const descendants = computed(() => {
  const ids = new Set<string>([props.groupId]);
  let grew = true;
  while (grew) {
    grew = false;
    for (const g of groups.value) {
      if (g.parentGroupId && ids.has(g.parentGroupId) && !ids.has(g.id)) {
        ids.add(g.id);
        grew = true;
      }
    }
  }
  return ids;
});
const parentOptions = computed(() => groups.value.filter((g) => !descendants.value.has(g.id)));

function move(parentGroupId: string) {
  if (!parentGroupId || parentGroupId === group.value?.parentGroupId) return;
  const parent = byId.value.get(parentGroupId)?.name;
  run(() => api.organizations.moveGroup(props.groupId, parentGroupId), `Grupo movido para baixo de ${parent}.`);
}

// ---------- Alçadas ----------
const assigned = computed(() => new Set(group.value?.rules ?? []));

/** Só os cargos do grupo aparecem; os de fora só no seletor de "Atribuir cargo". */
const groupRules = computed(() => ruleStore.alcadas.filter((r) => assigned.value.has(r.code)));
const unassignedRules = computed(() => ruleStore.alcadas.filter((r) => !assigned.value.has(r.code)));
const addingRule = ref(false);
const newRule = ref('');

function openAddRule() {
  newRule.value = unassignedRules.value[0]?.code ?? '';
  addingRule.value = true;
}

async function addRule() {
  if (!newRule.value) return;
  addingRule.value = false;
  await toggleAlcada(newRule.value, true);
}

function toggleAlcada(code: string, on: boolean) {
  const next = new Set(assigned.value);
  if (on) next.add(code);
  else next.delete(code);
  const name = ruleStore.nameOf(code);
  return run(() => api.organizations.setGroupRules(props.groupId, [...next]), on ? `Cargo ${name} atribuído ao grupo.` : `Cargo ${name} retirado do grupo.`);
}

/** Roles que o grupo concede (união das alçadas), agrupadas por área para leitura. */
const effective = computed(() => {
  const codes = new Set(ruleStore.alcadas.filter((r) => assigned.value.has(r.code)).flatMap((r) => r.roles));
  return ROLE_GROUPS.map((area) => ({ title: area.title, roles: area.roles.filter((r) => codes.has(r)) })).filter((a) => a.roles.length);
});
const effectiveCount = computed(() => effective.value.reduce((n, a) => n + a.roles.length, 0));
const isDecision = (code: string) => (DECISION_ROLES as string[]).includes(code);

// ---------- Membros ----------
const inGroup = computed(() => members.value.filter((m) => group.value?.memberIds.includes(m.id)));
const available = computed(() => members.value.filter((m) => !group.value?.memberIds.includes(m.id)));
const newMember = ref('');
const addingMember = ref(false);

function openAddMember() {
  newMember.value = available.value[0]?.id ?? '';
  addingMember.value = true;
}

function addMember() {
  const member = members.value.find((m) => m.id === newMember.value);
  if (!member) return;
  newMember.value = '';
  addingMember.value = false;
  run(() => api.organizations.addGroupMember(props.groupId, member.id), `${member.fullName} entrou no grupo.`);
}

async function removeRule(code: string) {
  const name = ruleStore.nameOf(code);
  const ok = await confirm({
    title: 'Retirar cargo',
    message: `Os membros do grupo perdem as regras do cargo ${name} (a menos que o recebam por outro grupo ou diretamente).`,
    confirmLabel: 'Retirar',
    danger: true,
  });
  if (ok) await toggleAlcada(code, false);
}

async function removeMember(member: Member) {
  const ok = await confirm({
    title: 'Tirar do grupo',
    message: `${member.fullName} continua na organização, mas perde os cargos deste grupo e a posição no organograma que ele dava.`,
    confirmLabel: 'Tirar do grupo',
    danger: true,
  });
  if (ok) run(() => api.organizations.removeGroupMember(props.groupId, member.id), `${member.fullName} saiu do grupo.`);
}

// ---------- Renomear / excluir ----------
const renaming = ref<string | null>(null);

async function rename() {
  const name = renaming.value?.trim();
  if (!name) return;
  renaming.value = null;
  await run(() => api.organizations.renameGroup(props.groupId, name), 'Grupo renomeado.');
}

async function remove() {
  const g = group.value!;
  const ok = await confirm({
    title: 'Excluir grupo',
    message: `Excluir "${g.name}"? Os ${inGroup.value.length} membro(s) continuam na organização e perdem os cargos deste grupo.`,
    confirmLabel: 'Excluir',
    danger: true,
  });
  if (!ok) return;
  try {
    await api.organizations.deleteGroup(props.groupId);
    toast.success('Grupo excluído.');
    router.push('/members');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

onMounted(load);
</script>

<template>
  <StateBlock :loading="loading && !group" :error="error ?? (!loading && !group ? 'Grupo não encontrado.' : null)" @retry="load">
    <template v-if="group">
      <PageHeader
        :kicker="isRoot ? 'Grupo · raiz do organograma' : `Grupo · nível ${group.depth} do organograma`"
        :title="group.name"
        subtitle="Tudo o que o grupo concede vale para todos os membros dele. Para aprovar, o membro também precisa estar num grupo acima de quem emitiu."
      >
        <template #actions>
          <template v-if="canEdit">
            <button class="btn" :disabled="busy" @click="renaming = group.name">Renomear</button>
            <button v-if="!isRoot" class="btn btn-danger" :disabled="busy" @click="remove">Excluir</button>
          </template>
        </template>
      </PageHeader>

      <section class="kpis" style="margin-bottom: 16px">
        <div class="kpi">
          <span>Membros</span>
          <strong>{{ inGroup.length }}</strong>
          <small>membro(s) no grupo</small>
        </div>
        <div class="kpi">
          <span>Cargos</span>
          <strong>{{ assigned.size }}</strong>
          <small>cargo(s) atribuído(s) ao grupo</small>
        </div>
        <div class="kpi">
          <span>Regras efetivas</span>
          <strong>{{ effectiveCount }}</strong>
          <small>regras concedidas aos membros</small>
        </div>
        <div class="kpi">
          <span>Organograma</span>
          <strong>{{ isRoot ? 'raiz' : `nível ${group.depth}` }}</strong>
          <small>{{ children.length }} grupo(s) abaixo</small>
        </div>
      </section>

      <div class="layout">
        <section class="card">
          <header class="card-header">
            <h2>Cargos do grupo</h2>
            <button v-if="canEdit && !organization.isInternalOrganization && unassignedRules.length" class="btn btn-primary btn-sm" :disabled="busy" @click="openAddRule">
              + Atribuir cargo
            </button>
          </header>
          <div class="card-body stack" style="gap: 8px">
            <p v-if="organization.isInternalOrganization" class="muted" style="margin: 0">A organização FIX não usa cargos: os papéis internos são fixos.</p>
            <p v-else-if="!ruleStore.alcadas.length" class="muted" style="margin: 0">
              A organização ainda não tem cargos. <RouterLink to="/access">Crie o primeiro</RouterLink> a partir das regras disponíveis.
            </p>
            <p v-else-if="!groupRules.length" class="muted" style="margin: 0">Nenhum cargo atribuído: os membros ficam só com a base (leitura).</p>
            <div v-for="rule in groupRules" :key="rule.code" class="alcada on">
              <span class="alcada-body">
                <strong>{{ rule.name }}</strong>
                <span class="roles">
                  <span v-for="code in rule.roles" :key="code" class="badge" :class="{ 'badge-warning': isDecision(code) }" :title="roleStore.describe(code)">{{ code }}</span>
                </span>
              </span>
              <button v-if="canEdit" class="btn btn-sm" :disabled="busy" @click="removeRule(rule.code)">Retirar</button>
            </div>
            <RouterLink to="/access" class="small manage">Gerir cargos e regras →</RouterLink>
          </div>
        </section>

        <section class="card">
          <header class="card-header">
            <h2>Membros</h2>
            <button v-if="canEdit && available.length" class="btn btn-primary btn-sm" :disabled="busy" @click="openAddMember">+ Adicionar membro</button>
          </header>
          <div class="card-body stack" style="gap: 10px">
            <p v-if="!inGroup.length" class="muted" style="margin: 0">Nenhum membro neste grupo.</p>
            <ul class="people">
              <li v-for="m in inGroup" :key="m.id">
                <span class="avatar">{{ m.fullName.charAt(0).toUpperCase() }}</span>
                <span class="who">
                  <strong>{{ m.fullName }}</strong>
                  <span>{{ m.email }}</span>
                </span>
                <span v-if="m.isOwner" class="badge badge-primary">Owner</span>
                <button v-if="canEdit && !(isRoot && m.isOwner)" class="btn btn-sm" :disabled="busy" @click="removeMember(m)">Tirar</button>
              </li>
            </ul>
          </div>
        </section>

        <section class="card">
          <header class="card-header">
            <h2>Regras efetivas</h2>
            <span class="muted small">o que cada membro recebe por estar aqui</span>
          </header>
          <div class="card-body">
            <p v-if="!effective.length" class="muted" style="margin: 0">Sem cargos: os membros ficam só com a base (leitura).</p>
            <dl v-else class="effective">
              <template v-for="a in effective" :key="a.title">
                <dt>{{ a.title }}</dt>
                <dd>
                  <span v-for="code in a.roles" :key="code" class="perm" :class="{ decision: isDecision(code) }">
                    {{ roleStore.describe(code) }}
                  </span>
                </dd>
              </template>
            </dl>
            <p v-if="effective.some((a) => a.roles.some(isDecision))" class="alert alert-info" style="margin: 12px 0 0">
              Em destaque, as decisões (aprovar, confirmar, alçada de emissão): valem para pedidos de quem está abaixo deste grupo no organograma.
            </p>
          </div>
        </section>

        <section class="card">
          <header class="card-header"><h2>Organograma</h2></header>
          <div class="card-body stack" style="gap: 12px">
            <ol class="path">
              <li v-for="(g, i) in path" :key="g.id" :style="{ '--depth': i }" :class="{ me: g.id === group.id }">
                <RouterLink v-if="g.id !== group.id" :to="paths.group(g.id)">{{ g.name }}</RouterLink>
                <strong v-else>{{ g.name }}</strong>
              </li>
              <li v-for="c in children" :key="c.id" class="child" :style="{ '--depth': path.length }">
                <RouterLink :to="paths.group(c.id)">{{ c.name }}</RouterLink>
              </li>
            </ol>
            <div v-if="canEdit && !isRoot" class="field">
              <label for="group-parent">Ficar abaixo de</label>
              <select id="group-parent" class="input" :value="group.parentGroupId ?? ''" :disabled="busy" @change="move(($event.target as HTMLSelectElement).value)">
                <option v-for="g in parentOptions" :key="g.id" :value="g.id">{{ '— '.repeat(g.depth) }}{{ g.name }}</option>
              </select>
            </div>
          </div>
        </section>
      </div>
    </template>
  </StateBlock>

  <BaseModal v-if="addingRule" title="Atribuir cargo ao grupo" width="460px" @close="addingRule = false">
    <form id="add-rule-form" class="stack" @submit.prevent="addRule">
      <p class="muted small" style="margin: 0">Todos os membros do grupo recebem as regras do cargo.</p>
      <div class="field">
        <label for="add-rule">Cargo</label>
        <select id="add-rule" v-model="newRule" class="input" required>
          <option v-for="rule in unassignedRules" :key="rule.code" :value="rule.code">{{ rule.name }} ({{ rule.roles.length }} regras)</option>
        </select>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="addingRule = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="add-rule-form" :disabled="!newRule || busy">Atribuir</button>
    </template>
  </BaseModal>

  <BaseModal v-if="addingMember" title="Adicionar membro ao grupo" width="460px" @close="addingMember = false">
    <form id="add-member-form" class="stack" @submit.prevent="addMember">
      <div class="field">
        <label for="add-member">Membro</label>
        <select id="add-member" v-model="newMember" class="input" required>
          <option v-for="m in available" :key="m.id" :value="m.id">{{ m.fullName }} · {{ m.email }}</option>
        </select>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="addingMember = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="add-member-form" :disabled="!newMember || busy">Adicionar</button>
    </template>
  </BaseModal>

  <BaseModal v-if="renaming !== null" title="Renomear grupo" width="420px" @close="renaming = null">
    <form id="rename-form" class="stack" @submit.prevent="rename">
      <div class="field">
        <label for="group-name">Nome</label>
        <input id="group-name" v-model="renaming" class="input" maxlength="150" required autofocus />
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="renaming = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="rename-form">Salvar</button>
    </template>
  </BaseModal>
</template>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  align-items: start;
}

@media (max-width: 1100px) {
  .layout {
    grid-template-columns: 1fr;
  }
}

.alcada {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  padding: 10px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  transition:
    border-color 0.15s,
    background 0.15s;
}

.alcada:hover {
  border-color: var(--primary-line);
}

.alcada.on {
  border-color: var(--primary);
  background: var(--primary-soft);
}

.alcada input {
  margin-top: 1px;
}

.alcada-body {
  display: flex;
  flex: 1;
  flex-direction: column;
  gap: 6px;
  min-width: 0;
}

.manage {
  align-self: flex-start;
  margin-top: 4px;
}

.roles {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.add {
  display: flex;
  gap: 8px;
}

.people {
  display: flex;
  flex-direction: column;
  margin: 0;
  padding: 0;
  list-style: none;
}

.people li {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 0;
  border-bottom: 1px solid var(--border);
}

.people li:last-child {
  border-bottom: 0;
}

.avatar {
  display: grid;
  place-items: center;
  flex: none;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: var(--surface-3);
  color: var(--text-dim);
  font-weight: 600;
  font-size: 0.85rem;
}

.who {
  display: flex;
  flex: 1;
  flex-direction: column;
  min-width: 0;
}

.who span {
  overflow: hidden;
  text-overflow: ellipsis;
  color: var(--text-muted);
  font-size: 0.8rem;
}

.effective {
  display: grid;
  grid-template-columns: 130px 1fr;
  gap: 10px 14px;
  margin: 0;
}

.effective dt {
  padding-top: 3px;
  font-size: 0.68rem;
  font-weight: 600;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--text-muted);
}

.effective dd {
  display: flex;
  flex-wrap: wrap;
  gap: 5px;
  margin: 0;
}

.perm {
  padding: 2px 9px;
  border: 1px solid var(--border-strong);
  border-radius: 999px;
  font-size: 0.8rem;
}

.perm.decision {
  border-color: var(--warning-line);
  background: var(--warning-soft);
  color: var(--warning);
  font-weight: 600;
}

.path {
  margin: 0;
  padding: 0;
  list-style: none;
}

.path li {
  position: relative;
  padding: 4px 0 4px calc(var(--depth) * 18px + 16px);
}

.path li::before {
  content: '↳';
  position: absolute;
  left: calc(var(--depth) * 18px);
  color: var(--primary);
  font-size: 0.8rem;
}

.path li:first-child::before {
  content: '●';
  font-size: 0.55rem;
  top: 9px;
}

.path li.me strong {
  color: var(--primary);
}

.path li.child a {
  color: var(--text-muted);
}

@media (max-width: 720px) {
  .effective {
    grid-template-columns: 1fr;
    gap: 4px;
  }

  .effective dd {
    margin-bottom: 8px;
  }
}
</style>
