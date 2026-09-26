import { onBeforeUnmount, onMounted, ref } from 'vue';
import { useApi } from '@/application/api-provider';
import type { FileProgress } from '@/domain/file';
import { ApiError } from '@/infrastructure/http/api-error';

/** Espera entre reconexões quando a conexão cai (BFF reiniciado, rede instável). */
const RETRY_MS = [1_000, 3_000, 5_000, 10_000];

/**
 * Progresso ao vivo do processamento dos uploads (SSE do BFF), enquanto a tela estiver aberta. Reconecta sozinho;
 * `live` indica se a conexão está aberta. Sem mensageria no servidor (503), para de tentar — a tela segue funcionando
 * com recarga manual.
 */
export function useFileProgress(fileId: string | null, onProgress: (event: FileProgress) => void) {
  const api = useApi();
  const live = ref(false);
  const controller = new AbortController();

  async function connect() {
    for (let attempt = 0; !controller.signal.aborted; attempt++) {
      try {
        await api.files.events(fileId, onProgress, controller.signal, () => {
          live.value = true;
          attempt = 0;
        });
      } catch (error) {
        if (controller.signal.aborted) return;
        if (error instanceof ApiError && [401, 403, 404, 503].includes(error.status)) {
          live.value = false;
          return;
        }
      }

      live.value = false;
      await new Promise((resolve) => setTimeout(resolve, RETRY_MS[Math.min(attempt, RETRY_MS.length - 1)]));
    }
  }

  onMounted(() => void connect());
  onBeforeUnmount(() => controller.abort());

  return { live };
}
