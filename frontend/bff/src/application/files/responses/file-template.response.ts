import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { FileTemplateColumnDto } from '../dtos/file-template.dto.js';

export interface FileTemplateResponse {
  kind: FileKind;
  /** false = só armazena (políticas, documentos). */
  processed: boolean;
  /** Vazio = qualquer extensão. */
  acceptedExtensions: string[];
  columns: FileTemplateColumnDto[];
}
