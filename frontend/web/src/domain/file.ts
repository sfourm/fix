/** Tipos de upload: usuários, políticas, mandatos e boletas são processados linha a linha (só criação); documentos só armazenados. */
export const FILE_KINDS = ['Users', 'Policies', 'Mandates', 'Orders', 'Documents'] as const;
export type FileKind = (typeof FILE_KINDS)[number];

export const FILE_STATUSES = ['Received', 'Processing', 'Completed', 'CompletedWithErrors', 'Failed', 'Stored'] as const;
export type FileStatus = (typeof FILE_STATUSES)[number];

export const FILE_LINE_STATUSES = ['Pending', 'Succeeded', 'Failed'] as const;
export type FileLineStatus = (typeof FILE_LINE_STATUSES)[number];

/** Limite por arquivo (o mesmo do storage-service). */
export const MAX_UPLOAD_BYTES = 20 * 1024 * 1024;

/** Tipos cujas linhas viram cadastros no core. */
export const PROCESSED_KINDS: readonly FileKind[] = ['Users', 'Policies', 'Mandates', 'Orders'];

/** Trecho da URL de cada tipo (/uploads/mandatos). */
export const fileKindSlug: Record<FileKind, string> = {
  Users: 'usuarios',
  Policies: 'politicas',
  Mandates: 'mandatos',
  Orders: 'boletas',
  Documents: 'documentos',
};

export const fileKindFromSlug = (slug: string | undefined): FileKind | null =>
  FILE_KINDS.find((kind) => fileKindSlug[kind] === slug) ?? null;

/** Arquivo em andamento: ainda recebe eventos de progresso. */
export const isFileRunning = (status: FileStatus): boolean => status === 'Received' || status === 'Processing';

export interface UploadedFile {
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

export interface FileLine {
  id: string;
  /** 1 = primeira linha de dados (o cabeçalho não conta). */
  number: number;
  status: FileLineStatus;
  /** Coluna → valor, como veio no arquivo. */
  values: Record<string, string>;
  /** Erro da linha ou resumo do que foi feito. */
  message: string | null;
  /** Código do que foi criado (MD-07, HX-0012). */
  resultCode: string | null;
  processedAt: string | null;
}

/** Card de um tipo na tela de uploads. */
export interface FileKindSummary {
  kind: FileKind;
  total: number;
  processing: number;
  withErrors: number;
  lastUploadedAt: string | null;
  /** O usuário tem a regra exigida para enviar este tipo. */
  canUpload: boolean;
}

export interface FileTemplateColumn {
  name: string;
  required: boolean;
  description: string;
  example: string;
}

export interface FileTemplate {
  kind: FileKind;
  /** false = só armazena. */
  processed: boolean;
  /** Vazio = qualquer extensão. */
  acceptedExtensions: string[];
  columns: FileTemplateColumn[];
}

/** Evento de progresso recebido ao vivo (SSE). */
export interface FileProgress {
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
  line: { number: number; status: FileLineStatus; message: string | null; resultCode: string | null } | null;
}

/** Aplica um evento de progresso ao arquivo carregado. */
export const applyProgress = (file: UploadedFile, event: FileProgress): UploadedFile => ({
  ...file,
  status: event.status,
  totalLines: event.totalLines,
  processedLines: event.processedLines,
  succeededLines: event.succeededLines,
  failedLines: event.failedLines,
  progressPercent: event.percent,
  error: event.error,
});
