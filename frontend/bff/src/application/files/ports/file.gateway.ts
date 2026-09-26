import type { Page } from '../../../cross-cutting/paging/page.js';
import type { UploadFileCommand } from '../commands/upload-file.command.js';
import type { FileDownloadUrlDto } from '../dtos/file-download-url.dto.js';
import type { FileKindSummaryDto } from '../dtos/file-kind-summary.dto.js';
import type { FileLineDto } from '../dtos/file-line.dto.js';
import type { FileTemplateDto } from '../dtos/file-template.dto.js';
import type { FileDto } from '../dtos/file.dto.js';
import type { GetFileDownloadUrlQuery } from '../queries/get-file-download-url.query.js';
import type { GetFileSummaryQuery } from '../queries/get-file-summary.query.js';
import type { GetFileTemplateQuery } from '../queries/get-file-template.query.js';
import type { GetFileQuery } from '../queries/get-file.query.js';
import type { ListFileLinesQuery } from '../queries/list-file-lines.query.js';
import type { ListFilesQuery } from '../queries/list-files.query.js';

/** Porta para o storage-service (gRPC FileService). */
export interface FileGateway {
  upload(command: UploadFileCommand): Promise<FileDto>;
  get(query: GetFileQuery): Promise<FileDto>;
  list(query: ListFilesQuery): Promise<Page<FileDto>>;
  listLines(query: ListFileLinesQuery): Promise<Page<FileLineDto>>;
  summary(query: GetFileSummaryQuery): Promise<FileKindSummaryDto[]>;
  template(query: GetFileTemplateQuery): Promise<FileTemplateDto>;
  downloadUrl(query: GetFileDownloadUrlQuery): Promise<FileDownloadUrlDto>;
}
