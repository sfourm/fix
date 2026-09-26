import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { FileStatus } from '../../../cross-cutting/enums/file-status.js';

/** Arquivo enviado (a URL interna do S3 não vai para o navegador; o download usa link temporário). */
export interface FileResponse {
  id: string;
  kind: FileKind;
  status: FileStatus;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  organizationId: string;
  uploadedBy: string;
  uploadedAt: string;
  finishedAt: string | null;
  totalLines: number;
  processedLines: number;
  succeededLines: number;
  failedLines: number;
  /** Falha do arquivo inteiro (ex.: não pôde ser lido). */
  error: string | null;
  /** 0–100 */
  progressPercent: number;
}
