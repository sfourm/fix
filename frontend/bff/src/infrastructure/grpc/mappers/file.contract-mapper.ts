import type { FileDownloadUrlDto } from '../../../application/files/dtos/file-download-url.dto.js';
import type { FileKindSummaryDto } from '../../../application/files/dtos/file-kind-summary.dto.js';
import type { FileLineDto } from '../../../application/files/dtos/file-line.dto.js';
import type { FileTemplateDto } from '../../../application/files/dtos/file-template.dto.js';
import type { FileDto } from '../../../application/files/dtos/file.dto.js';
import { nullable } from './common.contract-mapper.js';
import { fileKindEnum, fileLineStatusEnum, fileStatusEnum } from './enum.contract-mapper.js';

export interface ContractFile {
  id: string;
  kind: string;
  status: string;
  fileName: string;
  contentType: string;
  sizeBytes: string;
  organizationId: string;
  uploadedBy: string;
  uploadedAt: string;
  finishedAt?: string;
  totalLines: number;
  processedLines: number;
  succeededLines: number;
  failedLines: number;
  error?: string;
  storageUrl: string;
}

export interface ContractFileLine {
  id: string;
  number: number;
  status: string;
  values: Record<string, string>;
  message?: string;
  resultCode?: string;
  processedAt?: string;
}

export interface ContractFileKindSummary {
  kind: string;
  total: number;
  processing: number;
  withErrors: number;
  lastUploadedAt?: string;
  canUpload: boolean;
}

export interface ContractFileTemplate {
  kind: string;
  processed: boolean;
  acceptedExtensions: string[];
  columns: { name: string; required: boolean; description: string; example: string }[];
  exampleCsv: Buffer;
}

export const toFileDto = (f: ContractFile): FileDto => ({
  id: f.id,
  kind: fileKindEnum.fromContractRequired(f.kind),
  status: fileStatusEnum.fromContractRequired(f.status),
  fileName: f.fileName,
  contentType: f.contentType,
  sizeBytes: Number(f.sizeBytes),
  organizationId: f.organizationId,
  uploadedBy: f.uploadedBy,
  uploadedAt: f.uploadedAt,
  finishedAt: nullable(f.finishedAt),
  totalLines: f.totalLines,
  processedLines: f.processedLines,
  succeededLines: f.succeededLines,
  failedLines: f.failedLines,
  error: nullable(f.error),
  storageUrl: f.storageUrl,
});

export const toFileLineDto = (l: ContractFileLine): FileLineDto => ({
  id: l.id,
  number: l.number,
  status: fileLineStatusEnum.fromContractRequired(l.status),
  values: { ...l.values },
  message: nullable(l.message),
  resultCode: nullable(l.resultCode),
  processedAt: nullable(l.processedAt),
});

export const toFileKindSummaryDto = (k: ContractFileKindSummary): FileKindSummaryDto => ({
  kind: fileKindEnum.fromContractRequired(k.kind),
  total: k.total,
  processing: k.processing,
  withErrors: k.withErrors,
  lastUploadedAt: nullable(k.lastUploadedAt),
  canUpload: k.canUpload,
});

export const toFileTemplateDto = (t: ContractFileTemplate): FileTemplateDto => ({
  kind: fileKindEnum.fromContractRequired(t.kind),
  processed: t.processed,
  acceptedExtensions: t.acceptedExtensions,
  columns: t.columns.map((c) => ({ ...c })),
  exampleCsv: Buffer.from(t.exampleCsv ?? []),
});

export const toFileDownloadUrlDto = (d: { url: string; expiresAt: string }): FileDownloadUrlDto => ({
  url: d.url,
  expiresAt: d.expiresAt,
});
