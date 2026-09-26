import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { FileStatus } from '../../../cross-cutting/enums/file-status.js';

export interface FileDto {
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
  /** s3://bucket/chave */
  storageUrl: string;
}
