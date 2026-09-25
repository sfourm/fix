<script setup lang="ts">
import { useConfirm } from '../composables/useConfirm';
import BaseModal from './BaseModal.vue';

const { state, settle } = useConfirm();
</script>

<template>
  <BaseModal v-if="state.open" :title="state.title" width="440px" @close="settle(false)">
    <p style="margin: 0">{{ state.message }}</p>
    <template #footer>
      <button class="btn" type="button" @click="settle(false)">Cancelar</button>
      <button class="btn" :class="state.danger ? 'btn-danger' : 'btn-primary'" type="button" @click="settle(true)">
        {{ state.confirmLabel }}
      </button>
    </template>
  </BaseModal>
</template>
