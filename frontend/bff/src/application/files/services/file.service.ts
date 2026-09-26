import { AppError } from '../../../cross-cutting/errors/app-error.js';
import { mapPage, type Page } from '../../../cross-cutting/paging/page.js';
import type { UploadFileCommand } from '../commands/upload-file.command.js';
import type { FileContentDto } from '../dtos/file-content.dto.js';
import type { FileDownloadUrlDto } from '../dtos/file-download-url.dto.js';
import type { FileProgressDto } from '../dtos/file-progress.dto.js';
import {
  toFileKindSummaryResponse,
  toFileLineResponse,
  toFileResponse,
  toFileTemplateResponse,
} from '../mappers/file.mapper.js';
import type { FileProgressPort } from '../ports/file-progress.port.js';
import type { FileGateway } from '../ports/file.gateway.js';
import type { GetFileDownloadUrlQuery } from '../queries/get-file-download-url.query.js';
import type { GetFileSummaryQuery } from '../queries/get-file-summary.query.js';
import type { GetFileTemplateQuery } from '../queries/get-file-template.query.js';
import type { GetFileQuery } from '../queries/get-file.query.js';
import type { ListFileLinesQuery } from '../queries/list-file-lines.query.js';
import type { ListFilesQuery } from '../queries/list-files.query.js';
import type { WatchFileProgressQuery } from '../queries/watch-file-progress.query.js';
import type { FileKindSummaryResponse } from '../responses/file-kind-summary.response.js';
import type { FileLineResponse } from '../responses/file-line.response.js';
import type { FileTemplateResponse } from '../responses/file-template.response.js';
import type { FileResponse } from '../responses/file.response.js';

/** Uploads: o storage-service grava, valida e processa; o BFF repassa e acompanha o progresso. */
export class FileService {
  constructor(
    private readonly gateway: FileGateway,
    private readonly progress: FileProgressPort | null,
  ) {}

  async upload(command: UploadFileCommand): Promise<FileResponse> {
    return toFileResponse(await this.gateway.upload(command));
  }

  async get(query: GetFileQuery): Promise<FileResponse> {
    return toFileResponse(await this.gateway.get(query));
  }

  async list(query: ListFilesQuery): Promise<Page<FileResponse>> {
    return mapPage(await this.gateway.list(query), toFileResponse);
  }

  async listLines(query: ListFileLinesQuery): Promise<Page<FileLineResponse>> {
    return mapPage(await this.gateway.listLines(query), toFileLineResponse);
  }

  async summary(query: GetFileSummaryQuery): Promise<FileKindSummaryResponse[]> {
    return (await this.gateway.summary(query)).map(toFileKindSummaryResponse);
  }

  async template(query: GetFileTemplateQuery): Promise<FileTemplateResponse> {
    return toFileTemplateResponse(await this.gateway.template(query));
  }

  /** CSV de exemplo do tipo, para baixar e preencher no Excel. */
  async templateCsv(query: GetFileTemplateQuery): Promise<Buffer> {
    const template = await this.gateway.template(query);
    if (!template.processed) {
      throw AppError.validation('Este tipo aceita qualquer arquivo e não tem modelo.');
    }

    return template.exampleCsv;
  }

  downloadUrl(query: GetFileDownloadUrlQuery): Promise<FileDownloadUrlDto> {
    return this.gateway.downloadUrl(query);
  }

  /** Arquivo original, entregue pelo BFF (o storage confere as regras de quem pode ver). */
  content(query: GetFileQuery): Promise<FileContentDto> {
    return this.gateway.content(query);
  }

  /**
   * Progresso ao vivo: só da organização do usuário e dos tipos que ele pode ver (as regras são conferidas no storage
   * antes da inscrição). Devolve a função que encerra o acompanhamento.
   */
  async watch(query: WatchFileProgressQuery, listener: (event: FileProgressDto) => void): Promise<() => void> {
    if (!this.progress) {
      throw new AppError('unavailable', 'O acompanhamento ao vivo está indisponível (mensageria fora do ar).');
    }

    const visible = new Set((await this.gateway.summary({ context: query.context })).map((k) => k.kind));
    if (query.fileId) {
      await this.gateway.get({ context: query.context, id: query.fileId });
    }

    return this.progress.subscribe((event) => {
      if (
        event.organizationId === query.context.organizationId &&
        visible.has(event.kind) &&
        (!query.fileId || event.fileId === query.fileId)
      ) {
        listener(event);
      }
    });
  }
}
