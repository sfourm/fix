<template>
  <div class="auth">
    <div class="brand">
      <span class="logo">F</span>
      <span>Fix</span>
    </div>
    <main class="card auth-card">
      <div class="card-body">
        <RouterView />
      </div>
    </main>
  </div>
</template>

<style scoped>
.auth {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 20px;
  padding: 24px 16px;
}

.brand {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.4rem;
  font-weight: 700;
}

.logo {
  display: grid;
  place-items: center;
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: var(--primary);
  color: #fff;
}

.auth-card {
  width: 100%;
  max-width: 420px;
}
</style>
