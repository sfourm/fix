<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useApi } from '@/application/api-provider';
import { PROCESSED_KINDS, type FileKindSummary } from '@/domain/file';
import { fileKindLabel } from '@/domain/labels';
import { useLoader } from '../../composables/useAsync';
import PageHeader from '../../components/PageHeader.vue';
import StateBlock from '../../components/StateBlock.vue';
import { paths } from '../../paths';

const api = useApi();
const { data, loading, error, load } = useLoader(() => api.files.summary());

/** Um card por contexto que o usuário pode ver; os processados (viram cadastros) primeiro. */
const cards = computed<FileKindSummary[]>(() =>
  [...(data.value ?? [])].sort((a, b) => Number(!PROCESSED_KINDS.includes(a.kind)) - Number(!PROCESSED_KINDS.includes(b.kind))),
);

onMounted(load);
</script>

<template>
  <PageHeader
    title="Uploads"
    subtitle="Crie usuários, políticas, mandatos e boletas em lote a partir de planilhas — processadas linha a linha, sem que uma linha com erro afete as outras — ou armazene documentos da companhia."
  />

  <StateBlock :loading="loading && !data" :error="error" :empty="!!data && !cards.length" empty-text="Nenhum tipo de upload liberado para você." @retry="load">
    <section class="upload-cards">
      <RouterLink v-for="card in cards" :key="card.kind" :to="paths.uploadKind(card.kind)" class="upload-card card">
        <h2>{{ fileKindLabel[card.kind] }}</h2>
      </RouterLink>
    </section>
  </StateBlock>
</template>

<style scoped>
.upload-cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 16px;
}

.upload-card {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 96px;
  text-align: center;
  padding: 20px 22px;
  color: inherit;
  text-decoration: none;
  transition:
    transform 0.25s var(--ease),
    box-shadow 0.25s var(--ease),
    border-color 0.25s var(--ease);
}

.upload-card:hover,
.upload-card:focus-visible {
  transform: translateY(-2px);
  border-color: var(--primary-line);
  box-shadow: var(--shadow-hover);
  outline: none;
}

.upload-card h2 {
  font-size: 1.2rem;
}
</style>
