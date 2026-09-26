import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';

/** Card de um tipo na tela de uploads. */
export interface FileKindSummaryResponse {
  kind: FileKind;
  total: number;
  processing: number;
  withErrors: number;
  lastUploadedAt: string | null;
  /** O usuário tem a regra exigida para enviar este tipo. */
  canUpload: boolean;
}
