<script setup lang="ts">
import ThemeToggle from '../components/ThemeToggle.vue';
</script>

<template>
  <div class="auth">
    <div class="theme"><ThemeToggle /></div>
    <div class="brand">
      <span class="logo" aria-hidden="true">FIX</span>
      <h1>FIX</h1>
      <p>Gestão comercial e de risco</p>
    </div>
    <main class="auth-card">
      <RouterView />
    </main>
    <p class="foot">Plataforma FIX · acesso restrito</p>
  </div>
</template>

<style scoped>
.auth {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 26px;
  min-height: 100vh;
  min-height: 100dvh;
  padding: 72px 16px 32px;
}

.theme {
  position: absolute;
  top: max(16px, env(safe-area-inset-top));
  right: 16px;
}

.brand {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  text-align: center;
  animation: rise 0.6s var(--ease) both;
}

.logo {
  display: grid;
  place-items: center;
  width: 64px;
  height: 64px;
  margin-bottom: 10px;
  border-radius: 18px;
  background: linear-gradient(145deg, #2c2c2e, #1d1d1f);
  box-shadow: inset 0 0 0 0.5px rgb(255 255 255 / 14%), 0 12px 32px rgb(0 0 0 / 22%);
  color: #e0ae4d;
  font-family: var(--font-display);
  font-size: 1.15rem;
  font-weight: 750;
}

.brand h1 {
  font-size: clamp(2rem, 1.6rem + 1.6vw, 2.6rem);
}

.brand p {
  margin: 0;
  color: var(--text-muted);
  font-size: 1.05rem;
}

.auth-card {
  width: 100%;
  max-width: 420px;
  padding: 28px;
  border: 1px solid var(--border);
  border-radius: 24px;
  background: var(--glass);
  backdrop-filter: saturate(180%) blur(24px);
  -webkit-backdrop-filter: saturate(180%) blur(24px);
  box-shadow: var(--shadow-hover);
  animation: rise 0.6s 0.08s var(--ease) both;
}

.foot {
  margin: 0;
  color: var(--text-faint);
  font-size: 0.8rem;
}

@media (max-width: 480px) {
  .auth-card {
    padding: 22px 18px;
  }
}
</style>
