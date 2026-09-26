<script setup lang="ts">
import AuditPanel from '../../../components/audit/AuditPanel.vue';
import { usePolicyContext } from '../policy-context';

/** Auditoria só desta política: dados, parâmetros, eixos, bandas, instrumentos e versões, com filtros e exportação. */
const { policy } = usePolicyContext();
</script>

<template>
  <p class="lead small intro">
    Tudo o que mudou nesta política, com quem fez e quando. Mandatos e boletas têm o histórico próprio na página de cada um.
  </p>
  <AuditPanel entity-type="Policy" :entity-id="policy.id" :scope-label="policy.code.toUpperCase()" />
</template>

<style scoped>
.intro {
  margin: 0 0 14px;
}
</style>
