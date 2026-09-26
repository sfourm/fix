import type { FileKind, FileKindSummary, FileLine, FileLineStatus, FileProgress, FileTemplate, UploadedFile } from '@/domain/file';
import type { Page } from '@/domain/page';
import type { HttpClient } from '../http/http-client';
import type { PageQuery } from './page-query';

export function createFileApi(http: HttpClient) {
  return {
    summary: () => http.get<FileKindSummary[]>('/files/summary'),
    list: (kind: FileKind | null, query: PageQuery = {}) => http.get<Page<UploadedFile>>('/files', { kind, ...query }),
    get: (id: string) => http.get<UploadedFile>(`/files/${id}`),
    lines: (id: string, status: FileLineStatus | null, query: PageQuery = {}) =>
      http.get<Page<FileLine>>(`/files/${id}/lines`, { status, ...query }),
    template: (kind: FileKind) => http.get<FileTemplate>(`/files/templates/${kind}`),
    templateCsv: (kind: FileKind) => http.blob(`/files/templates/${kind}/csv`),
    downloadUrl: (id: string) => http.get<{ url: string; expiresAt: string }>(`/files/${id}/download`),
    /** Arquivo original entregue pelo BFF (o S3 do cluster não é público). */
    content: (id: string) => http.blob(`/files/${id}/content`),
    upload: (kind: FileKind, file: File) => http.upload<UploadedFile>('/files', file, { kind }),
    /** Progresso ao vivo (SSE): de um arquivo ou de todos os que o usuário pode ver. */
    events: (fileId: string | null, onProgress: (event: FileProgress) => void, signal: AbortSignal, onOpen?: () => void) =>
      http.stream(
        '/files/events',
        { fileId },
        (event, data) => {
          if (event === 'progress') onProgress(JSON.parse(data) as FileProgress);
        },
        signal,
        onOpen,
      ),
  };
}
