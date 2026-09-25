<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import { useRuleStore } from '@/application/stores/rule.store';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useSessionStore } from '@/application/stores/session.store';
import { deskLabel } from '@/domain/labels';
import { DESKS, type Desk, type Group, type Member } from '@/domain/organization';
import { Permission } from '@/domain/permissions';
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
const { confirm } = useConfirm();

const members = useLoader(() => api.organizations.members());
const groups = useLoader(() => api.organizations.groups());
const canEdit = computed(() => organization.can(Permission.EditOrganization));
const memberName = (id: string) => {
  const member = members.data.value?.find((m) => m.id === id);
  return member ? member.fullName || member.email : id.slice(0, 8);
};

// ---------- Adicionar membro ----------
const addingMember = ref(false);
const memberForm = reactive<{ email: string; ruleCode: string; desk: Desk | null }>({ email: '', ruleCode: 'operador', desk: 'ExecutionDesk' });
const memberSubmit = useSubmit();

async function addMember() {
  const updated = await memberSubmit.run(() => api.organizations.addMember(memberForm));
  if (updated) {
    members.data.value = updated;
    addingMember.value = false;
    memberForm.email = '';
    toast.success('Membro adicionado.');
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

// ---------- Grupos ----------
const creatingGroup = ref(false);
const groupForm = reactive<{ name: string; ruleCodes: string[] }>({ name: '', ruleCodes: [] });
const groupSubmit = useSubmit();

async function createGroup() {
  const updated = await groupSubmit.run(() => api.organizations.createGroup(groupForm));
  if (updated) {
    groups.data.value = updated;
    creatingGroup.value = false;
    Object.assign(groupForm, { name: '', ruleCodes: [] });
    toast.success('Grupo criado.');
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
  ruleStore.load().catch((e) => toast.error(errorMessage(e)));
});
</script>

<template>
  <PageHeader title="Membros e grupos" subtitle="As alçadas (roles) de cada membro vêm da rule atribuída a ele e das rules dos grupos em que está. A mesa indica a instância em que o membro atua." />

  <div class="stack">
    <section class="card">
      <header class="card-header">
        <h2>Membros</h2>
        <button v-if="canEdit" class="btn btn-primary btn-sm" @click="addingMember = true; memberSubmit.reset()">+ Adicionar membro</button>
      </header>
      <StateBlock :loading="members.loading.value && !members.data.value" :error="members.error.value" @retry="members.load">
        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Nome</th>
                <th>E-mail</th>
                <th>Mesa</th>
                <th>Rules</th>
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
                <td>
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
                  <div class="row">
                    <span v-for="rule in member.rules" :key="rule" class="badge" :class="rule === 'founder' ? 'badge-warning' : 'badge-primary'">
                      {{ ruleStore.nameOf(rule) }}
                    </span>
                  </div>
                </td>
                <td class="muted">{{ member.groups.join(', ') || '—' }}</td>
                <td class="actions">
                  <button
                    v-if="canEdit && !member.rules.includes('founder')"
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
        <h2>Grupos</h2>
        <button v-if="canEdit" class="btn btn-primary btn-sm" @click="creatingGroup = true; groupSubmit.reset()">+ Novo grupo</button>
      </header>
      <StateBlock :loading="groups.loading.value && !groups.data.value" :error="groups.error.value" @retry="groups.load">
        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Grupo</th>
                <th>Rules</th>
                <th>Membros</th>
                <th />
              </tr>
            </thead>
            <tbody>
              <tr v-for="group in groups.data.value ?? []" :key="group.id">
                <td>
                  <strong>{{ group.name }}</strong>
                  <span v-if="group.isDefault" class="badge" style="margin-left: 6px">padrão</span>
                </td>
                <td>
                  <div class="row">
                    <span v-for="rule in group.rules" :key="rule" class="badge badge-primary">{{ ruleStore.nameOf(rule) }}</span>
                    <span v-if="group.rules.length === 0" class="muted">—</span>
                  </div>
                </td>
                <td class="muted">{{ group.memberIds.map(memberName).join(', ') || '—' }}</td>
                <td class="actions">
                  <button v-if="canEdit" class="btn btn-sm" @click="openAddToGroup(group)">Adicionar membro</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </StateBlock>
    </section>
  </div>

  <BaseModal v-if="addingMember" title="Adicionar membro" @close="addingMember = false">
    <form id="member-form" class="stack" @submit.prevent="addMember">
      <p class="alert alert-info">O usuário precisa já ter uma conta criada com este e-mail.</p>
      <p v-if="memberSubmit.error.value" class="alert alert-error">{{ memberSubmit.error.value }}</p>
      <div class="field">
        <label for="member-email">E-mail</label>
        <input id="member-email" v-model="memberForm.email" class="input" type="email" required autofocus />
      </div>
      <div class="field">
        <label for="member-rule">Rule (alçada)</label>
        <select id="member-rule" v-model="memberForm.ruleCode" class="input">
          <option v-for="rule in ruleStore.assignable" :key="rule.code" :value="rule.code">{{ rule.name }}</option>
        </select>
      </div>
      <div class="field">
        <label for="member-desk">Mesa</label>
        <select id="member-desk" v-model="memberForm.desk" class="input">
          <option :value="null">—</option>
          <option v-for="desk in DESKS" :key="desk" :value="desk">{{ deskLabel[desk] }}</option>
        </select>
      </div>
    </form>
    <template #footer>
      <button class="btn" type="button" @click="addingMember = false">Cancelar</button>
      <button class="btn btn-primary" type="submit" form="member-form" :disabled="memberSubmit.submitting.value">Adicionar</button>
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
        <label>Rules do grupo</label>
        <div class="checkbox-list">
          <label v-for="rule in ruleStore.assignable" :key="rule.code">
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
