import { ref, shallowRef } from 'vue';
import { ApiError, errorMessage } from '@/infrastructure/http/api-error';

/** Carregamento de dados com estados de loading/erro. */
export function useLoader<T>(fetcher: () => Promise<T>) {
  const data = shallowRef<T | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const status = ref<number | null>(null);

  async function load() {
    loading.value = true;
    error.value = null;
    status.value = null;
    try {
      data.value = await fetcher();
    } catch (e) {
      error.value = errorMessage(e);
      status.value = e instanceof ApiError ? e.status : null;
    } finally {
      loading.value = false;
    }
  }

  return { data, loading, error, status, load };
}

/** Submissão de formulário: guarda erro geral e erros por campo vindos do BFF/core. */
export function useSubmit() {
  const submitting = ref(false);
  const error = ref<string | null>(null);
  const fieldErrors = ref<ApiError | null>(null);

  async function run<T>(action: () => Promise<T>): Promise<T | undefined> {
    submitting.value = true;
    error.value = null;
    fieldErrors.value = null;
    try {
      return await action();
    } catch (e) {
      error.value = errorMessage(e);
      fieldErrors.value = e instanceof ApiError ? e : null;
      return undefined;
    } finally {
      submitting.value = false;
    }
  }

  const fieldError = (name: string) => fieldErrors.value?.field(name);

  function reset() {
    error.value = null;
    fieldErrors.value = null;
  }

  return { submitting, error, run, fieldError, reset };
}
