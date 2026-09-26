import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UploadFileCommand {
  context: RequestContext;
  kind: FileKind;
  fileName: string;
  contentType: string;
  content: Buffer;
}
