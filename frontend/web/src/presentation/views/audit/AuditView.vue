<script setup lang="ts">
import AuditPanel from '../../components/audit/AuditPanel.vue';
import PageHeader from '../../components/PageHeader.vue';
</script>

<template>
  <PageHeader
    title="Auditoria"
    subtitle="Todas as alterações feitas na organização, com quem fez e quando. Alterações nas partes de um item (eixos, bandas, membros...) aparecem no item dono."
  />
  <AuditPanel />
</template>
