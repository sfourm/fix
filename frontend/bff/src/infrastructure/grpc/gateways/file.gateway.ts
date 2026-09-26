import type { UploadFileCommand } from '../../../application/files/commands/upload-file.command.js';
import type { FileContentDto } from '../../../application/files/dtos/file-content.dto.js';
import type { FileDownloadUrlDto } from '../../../application/files/dtos/file-download-url.dto.js';
import type { FileKindSummaryDto } from '../../../application/files/dtos/file-kind-summary.dto.js';
import type { FileLineDto } from '../../../application/files/dtos/file-line.dto.js';
import type { FileTemplateDto } from '../../../application/files/dtos/file-template.dto.js';
import type { FileDto } from '../../../application/files/dtos/file.dto.js';
import type { FileGateway } from '../../../application/files/ports/file.gateway.js';
import type { GetFileDownloadUrlQuery } from '../../../application/files/queries/get-file-download-url.query.js';
import type { GetFileSummaryQuery } from '../../../application/files/queries/get-file-summary.query.js';
import type { GetFileTemplateQuery } from '../../../application/files/queries/get-file-template.query.js';
import type { GetFileQuery } from '../../../application/files/queries/get-file.query.js';
import type { ListFileLinesQuery } from '../../../application/files/queries/list-file-lines.query.js';
import type { ListFilesQuery } from '../../../application/files/queries/list-files.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Page } from '../../../cross-cutting/paging/page.js';
import type { StorageClient } from '../storage-client.js';
import { toContractContext, toContractPage, toPage, type ContractPageInfo } from '../mappers/common.contract-mapper.js';
import { fileKindEnum, fileLineStatusEnum } from '../mappers/enum.contract-mapper.js';
import {
  toFileDownloadUrlDto,
  toFileDto,
  toFileKindSummaryDto,
  toFileLineDto,
  toFileTemplateDto,
  type ContractFile,
  type ContractFileKindSummary,
  type ContractFileLine,
  type ContractFileTemplate,
} from '../mappers/file.contract-mapper.js';

const SERVICE = 'FileService';

export class GrpcFileGateway implements FileGateway {
  constructor(private readonly storage: StorageClient) {}

  async upload({ context, kind, fileName, contentType, content }: UploadFileCommand): Promise<FileDto> {
    return toFileDto(
      await this.call<ContractFile>('UploadFile', context, {
        kind: fileKindEnum.toContract(kind),
        fileName,
        contentType,
        content,
      }),
    );
  }

  async get({ context, id }: GetFileQuery): Promise<FileDto> {
    return toFileDto(await this.call<ContractFile>('GetFile', context, { id }));
  }

  async list({ context, kind, page }: ListFilesQuery): Promise<Page<FileDto>> {
    const response = await this.call<{ files: ContractFile[]; page: ContractPageInfo | null }>('ListFiles', context, {
      kind: fileKindEnum.toContract(kind),
      page: toContractPage(page),
    });
    return toPage(response.files, response.page, toFileDto);
  }

  async listLines({ context, fileId, status, page }: ListFileLinesQuery): Promise<Page<FileLineDto>> {
    const response = await this.call<{ lines: ContractFileLine[]; page: ContractPageInfo | null }>(
      'ListFileLines',
      context,
      { fileId, status: fileLineStatusEnum.toContract(status), page: toContractPage(page) },
    );
    return toPage(response.lines, response.page, toFileLineDto);
  }

  async summary({ context }: GetFileSummaryQuery): Promise<FileKindSummaryDto[]> {
    const response = await this.call<{ kinds: ContractFileKindSummary[] }>('GetFileSummary', context, {});
    return response.kinds.map(toFileKindSummaryDto);
  }

  async template({ context, kind }: GetFileTemplateQuery): Promise<FileTemplateDto> {
    return toFileTemplateDto(
      await this.call<ContractFileTemplate>('GetFileTemplate', context, { kind: fileKindEnum.toContract(kind) }),
    );
  }

  async downloadUrl({ context, id }: GetFileDownloadUrlQuery): Promise<FileDownloadUrlDto> {
    return toFileDownloadUrlDto(await this.call<{ url: string; expiresAt: string }>('GetFileDownloadUrl', context, { id }));
  }

  async content({ context, id }: GetFileQuery): Promise<FileContentDto> {
    const response = await this.call<{ fileName: string; contentType: string; content: Buffer }>('GetFileContent', context, { id });
    return { fileName: response.fileName, contentType: response.contentType, content: Buffer.from(response.content ?? []) };
  }

  private call<T>(method: string, context: RequestContext, request: object): Promise<T> {
    return this.storage.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
