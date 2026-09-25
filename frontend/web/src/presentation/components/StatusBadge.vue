<script setup lang="ts">
import type { Tone } from '@/domain/labels';

/** Badge de status: recebe o texto e o tom já resolvidos pelos mapas de labels. */
defineProps<{ label: string; tone?: Tone }>();
</script>

<template>
  <span class="badge" :class="tone && tone !== 'neutral' ? `badge-${tone}` : ''">{{ label }}</span>
</template>
