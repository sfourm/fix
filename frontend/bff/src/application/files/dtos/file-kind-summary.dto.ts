import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';

export interface FileKindSummaryDto {
  kind: FileKind;
  total: number;
  processing: number;
  withErrors: number;
  lastUploadedAt: string | null;
  /** O usuário tem a regra exigida para enviar este tipo. */
  canUpload: boolean;
}
