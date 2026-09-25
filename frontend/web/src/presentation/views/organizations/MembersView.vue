<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useApi } from '@/application/api-provider';
import { useRuleStore } from '@/application/stores/rule.store';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useSessionStore } from '@/application/stores/session.store';
import { deskLabel } from '@/domain/labels';
import { DESKS, type Desk, type Group, type Member } from '@/domain/organization';
import { Permission } from '@/domain/permissions';
import { baseRoleLabel } from '@/domain/rule';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useLoader, useSubmit } from '../../composables/useAsync';
import { useConfirm } from '../../composables/useConfirm';
import { useToast } from '../../composables/useToast';
import BaseModal from '../../components/BaseModal.vue';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';

const api = useApi();
const organization = useOrganizationStore();
const ruleStore = useRuleStore();
const session = useSessionStore();
const toast = useToast();
const router = useRouter();
const { confirm } = useConfirm();

const members = useLoader(() => api.organizations.members());
const groups = useLoader(() => api.organizations.groups());
const canEdit = computed(() => organization.can(Permission.EditOrganization));
/** Na organização FIX a tela gere a equipe interna: sem alçadas, sem owner. */
const internalTeam = computed(() => organization.isInternalOrganization);
const memberName = (id: string) => {
  const member = members.data.value?.find((m) => m.id === id);
  return member ? member.fullName || member.email : id.slice(0, 8);
};

/** Só o owner atual ou a equipe interna FIX (em suporte) transferem a propriedade. */
const me = computed(() => members.data.value?.find((m) => m.userId === session.user?.id) ?? null);
const canTransfer = computed(() => !internalTeam.value && (me.value?.isOwner || organization.internalAccess));

// ---------- Adicionar membro ----------
const addingMember = ref(false);
const memberForm = reactive<{ email: string; ruleCode: string | null; desk: Desk | null }>({ email: '', ruleCode: null, desk: null });
const memberSubmit = useSubmit();

async function addMember() {
  const updated = await memberSubmit.run(() => api.organizations.addMember({ ...memberForm, ruleCode: internalTeam.value ? null : memberForm.ruleCode }));
  if (updated) {
    members.data.value = updated;
    addingMember.value = false;
    memberForm.email = '';
    await ruleStore.load(true);
    toast.success(internalTeam.value ? 'Administrador adicionado à equipe FIX.' : 'Membro adicionado.');
  }
}

async function changeDesk(member: Member, desk: Desk | null) {
  try {
    members.data.value = await api.organizations.changeMemberDesk(member.id, desk);
    toast.success('Mesa atualizada.');
  } catch (e) {
    toast.error(errorMessage(e));
    await members.load();
  }
}

function onDeskChange(member: Member, event: Event) {
  const value = (event.target as HTMLSelectElement).value;
  changeDesk(member, value ? (value as Desk) : null);
}

async function removeMember(member: Member) {
  const ok = await confirm({
    title: 'Remover membro',
    message: `Remover ${member.fullName || member.email} da organização?`,
    confirmLabel: 'Remover',
    danger: true,
  });
  if (!ok) return;

  try {
    await api.organizations.removeMember(member.id);
    toast.success('Membro removido.');
    await Promise.all([members.load(), groups.load()]);
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

async function transferOwnership(member: Member) {
  const ok = await confirm({
    title: 'Transferir propriedade',
    message: `${member.fullName || member.email} passa a ser o owner da organização e o owner atual vira usuário (mantém as alçadas que tiver).`,
    confirmLabel: 'Transferir',
    danger: true,
  });
  if (!ok) return;

  try {
    await api.organizations.transferOwnership(member.id);
    await organization.refresh();
    toast.success('Propriedade transferida.');
    // Quem transferiu vira usuário e pode perder o acesso a esta tela.
    if (organization.can(Permission.ViewUsers)) await members.load();
    else await router.push('/');
  } catch (e) {
    toast.error(errorMessage(e));
  }
}

// ---------- Alçadas de membro ou grupo ----------
const editingRules = ref<{ kind: 'member' | 'group'; id: string; name: string } | null>(null);
const selectedRules = ref<string[]>([]);
const rulesSubmit = useSubmit();

function openRules(kind: 'member' | 'group', target: Member | Group) {
  editingRules.value = { kind, id: target.id, name: 'fullName' in target ? target.fullName || target.email : target.name };
  selectedRules.value = [...target.rules];
  rulesSubmit.reset();
}

async function saveRules() {
  const target = editingRules.value!;
  const ok = await rulesSubmit.run(async () => {
    if (target.kind === 'member') members.data.value = await api.organizations.setMemberRules(target.id, selectedRules.value);
    else groups.data.value = await api.organizations.setGroupRules(target.id, selectedRules.value);
    return true;
  });
  if (ok) {
    editingRules.value = null;
    await Promise.all([ruleStore.load(true), organization.refresh()]);
    toast.success('Alçadas atualizadas.');
  }
}

// ---------- Grupos ----------
const creatingGroup = ref(false);
const groupForm = reactive<{ name: string; ruleCodes: string[]; parentGroupId: string | null }>({ name: '', ruleCodes: [], parentGroupId: null });
const groupSubmit = useSubmit();

async function createGroup() {
  const updated = await groupSubmit.run(() => api.organizations.createGroup(groupForm));
  if (updated) {
    groups.data.value = updated;
    creatingGroup.value = false;
    Object.assign(groupForm, { name: '', ruleCodes: [], parentGroupId: null });
    toast.success('Grupo criado.');
  }
}

// ---------- Organograma ----------
/** Grupos em ordem de árvore (cada grupo seguido dos que estão abaixo dele), para exibir o organograma. */
const chart = computed(() => {
  const all = groups.data.value ?? [];
  const children = (parentId: string | null) =>
    all.filter((g) => g.parentGroupId === parentId).sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'));
  const ordered: Group[] = [];
  const visit = (group: Group) => {
    ordered.push(group);
    children(group.id).forEach(visit);
  };
  children(null).forEach(visit);
  return ordered;
});
const groupName = (id: string | null) => (groups.data.value ?? []).find((g) => g.id === id)?.name ?? '—';

/** Um grupo não pode ir para baixo de si mesmo nem de um grupo que já está abaixo dele. */
function descendantsOf(groupId: string): Set<string> {
  const result = new Set<string>();
  const walk = (id: string) =>
    (groups.data.value ?? []).filter((g) => g.parentGroupId === id).forEach((g) => {
      result.add(g.id);
      walk(g.id);
    });
  walk(groupId);
  return result;
}

const movingGroup = ref<Group | null>(null);
const moveParentId = ref('');
const moveSubmit = useSubmit();
const moveTargets = computed(() => {
  const moving = movingGroup.value;
  if (!moving) return [];
  const blocked = descendantsOf(moving.id);
  return chart.value.filter((g) => g.id !== moving.id && !blocked.has(g.id));
});

function openMove(group: Group) {
  movingGroup.value = group;
  moveParentId.value = group.parentGroupId ?? '';
  moveSubmit.reset();
}

async function moveGroup() {
  const group = movingGroup.value;
  if (!group || !moveParentId.value) return;
  const updated = await moveSubmit.run(() => api.organizations.moveGroup(group.id, moveParentId.value));
  if (updated) {
    groups.data.value = updated;
    movingGroup.value = null;
    toast.success('Organograma atualizado.');
  }
}

const addingToGroup = ref<Group | null>(null);
const selectedMemberId = ref('');
const groupMemberSubmit = useSubmit();
const availableMembers = computed(() =>
  (members.data.value ?? []).filter((m) => !addingToGroup.value?.memberIds.includes(m.id)),
);

function openAddToGroup(group: Group) {
  addingToGroup.value = group;
  selectedMemberId.value = availableMembers.value[0]?.id ?? '';
  groupMemberSubmit.reset();
}

async function addToGroup() {
  if (!addingToGroup.value || !selectedMemberId.value) return;
  const groupId = addingToGroup.value.id;
  const updated = await groupMemberSubmit.run(() => api.organizations.addGroupMember(groupId, selectedMemberId.value));
  if (updated) {
    groups.data.value = updated;
    addingToGroup.value = null;
    await members.load();
    await organization.refresh();
    toast.success('Membro adicionado ao grupo.');
  }
}

onMounted(() => {
  members.load();
  groups.load();
  ruleStore.load(true).catch((e) => toast.error(errorMessage(e)));
});
</script>

<template>
  <PageHeader
    :title="internalTeam ? 'Equipe FIX' : 'Membros e grupos'"
    :subtitle="
      internalTeam
        ? 'Equipe interna: o super administrador adiciona e controla os administradores, que dão suporte às organizações clientes (veem e editam, sem decidir).'
        : 'Toda organização tem um owner; os demais membros são usuários. As permissões extras vêm das alçadas atribuídas ao membro e aos grupos em que ele está.'
    "
  />

  <div class="stack">
    <section class="card">
      <header class="card-header">
        <h2>{{ internalTeam ? 'Administradores' : 'Membros' }}</h2>
        <button v-if="canEdit" class="btn btn-primary btn-sm" @click="addingMember = true; memberSubmit.reset()">
          {{ internalTeam ? '+ Adicionar administrador' : '+ Adicionar membro' }}
        </button>
      </header>
      <StateBlock :loading="members.loading.value && !members.data.value" :error="members.error.value" @retry="members.load">
        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>E-mail</th>
                <th v-if="!internalTeam">Mesa</th>
                <th>Papel</th>
                <th v-if="!internalTeam">Alçadas</th>
                <th>Grupos</th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr v-for="member in members.data.value ?? []" :key="member.id">
                <td>
                  <strong>{{ member.fullName }}</strong>
                  <span v-if="member.userId === session.user?.id" class="badge" style="margin-left: 6px">você</span>
                </td>
                <td class="muted">{{ member.email }}</td>
                <td v-if="!internalTeam">
                  <select
                    v-if="canEdit"
                    class="input input-sm"
                    :value="member.desk ?? ''"
                    :aria-label="`Mesa de ${member.fullName}`"
                    @change="onDeskChange(member, $event)"
                  >
                    <option value="">—</option>
                    <option v-for="desk in DESKS" :key="desk" :value="desk">{{ deskLabel[desk] }}</option>
                  </select>
                  <span v-else>{{ member.desk ? deskLabel[member.desk] : '—' }}</span>
                </td>
                <td>
                  <span class="badge" :class="member.isOwner || member.role === 'super_administrador' ? 'badge-warning' : ''">
                    {{ baseRoleLabel[member.role] ?? member.role }}
                  </span>
                </td>
                <td v-if="!internalTeam">
                  <div class="row">
                    <span v-for="rule in member.rules" :key="rule" class="badge badge-primary">{{ ruleStore.nameOf(rule) }}</span>
                    <span v-if="member.rules.length === 0" class="muted">—</span>
                  </div>
                </td>
                <td class="muted">{{ member.groups.join(', ') || '—' }}</td>
                <td class="actions">
                  <button v-if="canEdit && !internalTeam" class="btn btn-sm" @click="openRules('member', member)">Alçadas</button>
                  <button v-if="canTransfer && !member.isOwner" class="btn btn-sm" @click="transferOwnership(member)">Tornar owner</button>
                  <button
                    v-if="canEdit && !member.isOwner && member.role !== 'super_administrador'"
                    class="btn btn-sm btn-danger"
                    @click="removeMember(member)"
                  >
                    Remover
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </StateBlock>
    </section>

    <section class="card">
      <header class="card-header">
        <div class="stack" style="gap: 2px">
          <h2>Grupos e organograma</h2>
          <span class="muted small">Mandatos e boletas só são aprovados ou rejeitados por quem está num grupo acima de quem os emitiu.</span>
        </div>
        <button v-if="canEdit" class="btn btn-primary btn-sm" @click="creatingGroup = true; groupSubmit.reset()">+ Novo grupo</button>
      </header>
      <StateBlock :loading="groups.loading.value && !groups.data.value" :error="groups.error.value" @retry="groups.load">
        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Grupo</th>
                <th>Acima</th>
                <th v-if="!internalTeam">Alçadas</th>
                <th>Membros</th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr v-for="group in chart" :key="group.id">
                <td>
                  <span class="org-node" :style="{ paddingLeft: `${group.depth * 20}px` }">
                    <span v-if="group.depth > 0" class="muted" aria-hidden="true">└</span>
                    <strong>{{ group.name }}</strong>
                    <span v-if="group.depth === 0" class="badge badge-info">raiz</span>
                  </span>
                </td>
                <td class="muted">{{ group.parentGroupId ? groupName(group.parentGroupId) : '—' }}</td>
                <td v-if="!internalTeam">
                  <div class="row">
                    <span v-for="rule in group.rules" :key="rule" class="badge badge-primary">{{ ruleStore.nameOf(rule) }}</span>
                    <span v-if="group.rules.length === 0" class="muted">—</span>
                  </div>
                </td>
                <td class="muted">{{ group.memberIds.map(memberName).join(', ') || '—' }}</td>
                <td class="actions">
                  <template v-if="canEdit">
                    <button v-if="!internalTeam" class="btn btn-sm" @click="openRules('group', group)">Alçadas</button>
                    <button v-if="group.depth > 0" class="btn btn-sm" @click="openMove(group)">Mover</button>
                    <button class="btn btn-sm" @click="openAddToGroup(group)">Adicionar membro</button>
                  </template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </StateBlock>
    </section>
  </div>

  <BaseModal v-if="addingMember" :title="internalTeam ? 'Adicionar administrador' : 'Adicionar membro'" @close="addingMember = false">
    <form id="member-form" class="stack" @submit.prevent="addMember">
      <p class="alert alert-info">
        {{ internalTeam ? 'O administrador interno passa a ver e editar as organizações clientes, sem decidir por elas.' : 'O usuário precisa já ter uma conta criada com este e-mail. Ele entra como usuário.' }}
      </p>
      <p v-if="memberSubmit.error.value" class="alert alert-error">{{ memberSubmit.error.value }}</p>
      <div class="field">
        <label for="member-email">E-mail</label>
        <input id="member-email" v-model="memberForm.email" class="input" type="email" required autofocus />
      </div>
      <template v-if="!internalTeam">
        <div class="field">
          <label for="member-rule">Alçada inicial</label>
          <select id="member-rule" v-model="memberForm.ruleCode" class="input">
            <option :value="null">Nenhuma (só usuário)</option>
            <option v-for="rule in ruleStore.alcadas" :key="rule.code" :value="rule.code">{{ rule.name }}</option>
          </select>
        </div>
        <div class="field">
          <label for="member-desk">Mesa</label>
          <select id="member-desk" v-model="memberForm.desk" class="input">
            <option :value="null">—</option>
            <option v-for="desk in DESKS" :key="desk" :value="desk">{{ deskLabel[desk] }}</option>
          </select>
        </div>
      </template>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="addingMember = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="member-form" :disabled="memberSubmit.submitting.value">Adicionar</button>
    </template>
  </BaseModal>

  <BaseModal v-if="editingRules" :title="`Alçadas de ${editingRules.name}`" width="480px" @close="editingRules = null">
    <form id="rules-form" class="stack" @submit.prevent="saveRules">
      <p class="muted small" style="margin: 0">
        {{ editingRules.kind === 'group' ? 'Todos os membros do grupo recebem as roles destas alçadas.' : 'Somam-se às alçadas herdadas dos grupos do membro.' }}
        As alçadas são definidas em <RouterLink to="/access">Rules e alçadas</RouterLink>.
      </p>
      <p v-if="rulesSubmit.error.value" class="alert alert-error">{{ rulesSubmit.error.value }}</p>
      <div class="checkbox-list stack" style="gap: 8px">
        <label v-for="rule in ruleStore.alcadas" :key="rule.code">
          <input v-model="selectedRules" type="checkbox" :value="rule.code" />
          {{ rule.name }} <span class="muted small">({{ rule.roles.length }} roles)</span>
        </label>
        <p v-if="ruleStore.alcadas.length === 0" class="muted">Nenhuma alçada criada ainda.</p>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="editingRules = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="rules-form" :disabled="rulesSubmit.submitting.value">Salvar</button>
    </template>
  </BaseModal>

  <BaseModal v-if="movingGroup" :title="`Mover ${movingGroup.name} no organograma`" width="460px" @close="movingGroup = null">
    <form id="move-group-form" class="stack" @submit.prevent="moveGroup">
      <p class="muted small" style="margin: 0">Os grupos abaixo de {{ movingGroup.name }} vão junto. Quem decide pelos membros deles muda conforme a nova posição.</p>
      <p v-if="moveSubmit.error.value" class="alert alert-error">{{ moveSubmit.error.value }}</p>
      <div class="field">
        <label for="move-parent">Ficar abaixo de</label>
        <select id="move-parent" v-model="moveParentId" class="input" required>
          <option v-for="g in moveTargets" :key="g.id" :value="g.id">{{ '— '.repeat(g.depth) }}{{ g.name }}</option>
        </select>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="movingGroup = null">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="move-group-form" :disabled="moveSubmit.submitting.value || moveParentId === movingGroup.parentGroupId">Mover</button>
    </template>
  </BaseModal>

  <BaseModal v-if="creatingGroup" title="Novo grupo" @close="creatingGroup = false">
    <form id="group-form" class="stack" @submit.prevent="createGroup">
      <p v-if="groupSubmit.error.value" class="alert alert-error">{{ groupSubmit.error.value }}</p>
      <div class="field">
        <label for="group-name">Nome</label>
        <input id="group-name" v-model="groupForm.name" class="input" maxlength="150" required autofocus />
      </div>
      <div class="field">
        <label for="group-parent">Abaixo de (organograma)</label>
        <select id="group-parent" v-model="groupForm.parentGroupId" class="input">
          <option :value="null">{{ chart[0]?.name ?? 'Raiz' }} (raiz)</option>
          <option v-for="g in chart.filter((x) => x.depth > 0)" :key="g.id" :value="g.id">{{ '— '.repeat(g.depth) }}{{ g.name }}</option>
        </select>
      </div>
      <div v-if="!internalTeam" class="field">
        <label>Alçadas do grupo</label>
        <div class="checkbox-list">
          <label v-for="rule in ruleStore.alcadas" :key="rule.code">
            <input v-model="groupForm.ruleCodes" type="checkbox" :value="rule.code" />
            {{ rule.name }}
          </label>
        </div>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="creatingGroup = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="group-form" :disabled="groupSubmit.submitting.value">Criar</button>
    </template>
  </BaseModal>

  <BaseModal v-if="addingToGroup" :title="`Adicionar ao grupo ${addingToGroup.name}`" @close="addingToGroup = null">
    <form id="group-member-form" class="stack" @submit.prevent="addToGroup">
      <p v-if="groupMemberSubmit.error.value" class="alert alert-error">{{ groupMemberSubmit.error.value }}</p>
      <p v-if="availableMembers.length === 0" class="muted">Todos os membros já estão neste grupo.</p>
      <div v-else class="field">
        <label for="group-member">Membro</label>
        <select id="group-member" v-model="selectedMemberId" class="input">
          <option v-for="member in availableMembers" :key="member.id" :value="member.id">{{ member.fullName }} ({{ member.email }})</option>
        </select>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="addingToGroup = null">Cancelar</button>
      <button
        class="btn btn-primary"
        type="submit"
        form="group-member-form"
        :disabled="groupMemberSubmit.submitting.value || availableMembers.length === 0"
      >
        Adicionar
      </button>
    </template>
  </BaseModal>
</template>

<style scoped>
.org-node {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}
</style>
