import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { FileLineStatus } from '../../../cross-cutting/enums/file-line-status.js';
import type { FileStatus } from '../../../cross-cutting/enums/file-status.js';

/** Evento file.progress do storage-service (RabbitMQ), repassado ao navegador por SSE. */
export interface FileProgressDto {
  fileId: string;
  organizationId: string;
  userId: string;
  kind: FileKind;
  fileName: string;
  status: FileStatus;
  totalLines: number;
  processedLines: number;
  succeededLines: number;
  failedLines: number;
  percent: number;
  error: string | null;
  /** A linha que acabou de terminar (ausente nos eventos do arquivo inteiro). */
  line: {
    number: number;
    status: FileLineStatus;
    message: string | null;
    resultCode: string | null;
  } | null;
}
