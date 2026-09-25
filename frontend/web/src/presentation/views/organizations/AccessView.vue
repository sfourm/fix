<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useOrganizationStore } from '@/application/stores/organization.store';
import { useRoleStore } from '@/application/stores/role.store';
import { useRuleStore } from '@/application/stores/rule.store';
import { Permission } from '@/domain/permissions';
import { errorMessage } from '@/infrastructure/http/api-error';
import { useToast } from '../../composables/useToast';
import PageHeader from '../../components/PageHeader.vue';
import StatusBadge from '../../components/StatusBadge.vue';

const organization = useOrganizationStore();
const roleStore = useRoleStore();
const ruleStore = useRuleStore();
const toast = useToast();

const roles = computed(() => roleStore.roles.map((r) => ({ ...r, granted: organization.roles.has(r.code) })));
const hasSelfApprove = computed(() => organization.can(Permission.SelfApprove));

onMounted(() => {
  Promise.all([roleStore.load(), ruleStore.load()]).catch((e) => toast.error(errorMessage(e)));
});
</script>

<template>
  <PageHeader
    title="Rules e alçadas"
    subtitle="Roles são permissões atômicas; rules agrupam roles e são atribuídas a membros e grupos. A autorização acontece no core, consultando a base."
  />

  <div class="grid-2">
    <section class="card">
      <header class="card-header">
        <h2>Suas roles nesta organização</h2>
        <StatusBadge :label="hasSelfApprove ? 'Com alçada de emissão' : 'Sem alçada de emissão'" :tone="hasSelfApprove ? 'success' : 'warning'" />
      </header>
      <div class="table-wrap">
        <table class="table">
          <tbody>
            <tr v-for="role in roles" :key="role.code">
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

    <section class="card">
      <header class="card-header"><h2>Rules do sistema</h2></header>
      <div class="table-wrap">
        <table class="table">
          <tbody>
            <tr v-for="rule in ruleStore.rules" :key="rule.code">
              <td>
                <strong>{{ rule.name }}</strong>
                <div class="muted small"><code>{{ rule.code }}</code> · {{ rule.roles.length }} roles</div>
                <div class="row" style="margin-top: 6px">
                  <span v-for="code in rule.roles" :key="code" class="badge" :title="roleStore.describe(code)">{{ code }}</span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>
