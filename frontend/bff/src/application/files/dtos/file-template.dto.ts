import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';

export interface FileTemplateColumnDto {
  name: string;
  required: boolean;
  description: string;
  example: string;
}

export interface FileTemplateDto {
  kind: FileKind;
  /** false = só armazena (políticas, documentos). */
  processed: boolean;
  /** Vazio = qualquer extensão. */
  acceptedExtensions: string[];
  columns: FileTemplateColumnDto[];
  /** CSV de exemplo (UTF-8 com BOM); vazio nos tipos só armazenados. */
  exampleCsv: Buffer;
}
