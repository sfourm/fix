import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Acompanhamento ao vivo do processamento (SSE). */
export interface WatchFileProgressQuery {
  context: RequestContext;
  /** Só este arquivo (tela de detalhe) ou todos os que o usuário pode ver (lista). */
  fileId: string | null;
}
