<script setup lang="ts">
import { reactive } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useSessionStore } from '@/application/stores/session.store';
import { useSubmit } from '../../composables/useAsync';

const session = useSessionStore();
const router = useRouter();
const route = useRoute();

const form = reactive({ email: '', password: '' });
const { submitting, error, run } = useSubmit();

async function submit() {
  await run(() => session.login(form));
  if (session.isAuthenticated) {
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/';
    router.push(redirect);
  }
}
</script>

<template>
  <form class="stack" @submit.prevent="submit">
    <div>
      <h1>Entrar</h1>
      <p class="muted" style="margin: 4px 0 0">Acesse sua conta para continuar.</p>
    </div>
    <p v-if="error" class="alert alert-error">{{ error }}</p>
    <div class="field">
      <label for="email">E-mail</label>
      <input id="email" v-model="form.email" class="input" type="email" autocomplete="email" required autofocus />
    </div>
    <div class="field">
      <label for="password">Senha</label>
      <input id="password" v-model="form.password" class="input" type="password" autocomplete="current-password" required />
    </div>
    <button class="btn btn-primary btn-block" type="submit" :disabled="submitting">
      {{ submitting ? 'Entrando…' : 'Entrar' }}
    </button>
    <p class="muted small" style="margin: 0; text-align: center">
      Não tem conta? <RouterLink to="/register">Criar conta</RouterLink>
    </p>
  </form>
</template>
