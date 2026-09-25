<script setup lang="ts">
import { reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useSessionStore } from '@/application/stores/session.store';
import { useSubmit } from '../../composables/useAsync';

const session = useSessionStore();
const router = useRouter();

const form = reactive({ fullName: '', email: '', password: '' });
const { submitting, error, run, fieldError } = useSubmit();

async function submit() {
  await run(() => session.register(form));
  if (session.isAuthenticated) {
    router.push('/organizations');
  }
}
</script>

<template>
  <form class="stack" @submit.prevent="submit">
    <div>
      <h1>Criar conta</h1>
      <p class="muted" style="margin: 4px 0 0">Depois você cria sua organização ou é convidado para uma.</p>
    </div>
    <p v-if="error" class="alert alert-error">{{ error }}</p>
    <div class="field">
      <label for="fullName">Nome completo</label>
      <input id="fullName" v-model="form.fullName" class="input" autocomplete="name" maxlength="150" required autofocus />
      <span v-if="fieldError('fullName')" class="field-error">{{ fieldError('fullName') }}</span>
    </div>
    <div class="field">
      <label for="email">E-mail</label>
      <input id="email" v-model="form.email" class="input" type="email" autocomplete="email" required />
      <span v-if="fieldError('email')" class="field-error">{{ fieldError('email') }}</span>
    </div>
    <div class="field">
      <label for="password">Senha</label>
      <input id="password" v-model="form.password" class="input" type="password" autocomplete="new-password" minlength="8" required />
      <span class="muted small">Mínimo de 8 caracteres, com letra maiúscula, minúscula e número.</span>
      <span v-if="fieldError('password')" class="field-error">{{ fieldError('password') }}</span>
    </div>
    <button class="btn btn-primary btn-block" type="submit" :disabled="submitting">
      {{ submitting ? 'Criando…' : 'Criar conta' }}
    </button>
    <p class="muted small" style="margin: 0; text-align: center">
      Já tem conta? <RouterLink to="/login">Entrar</RouterLink>
    </p>
  </form>
</template>
