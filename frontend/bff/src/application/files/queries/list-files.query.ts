import type { FileKind } from '../../../cross-cutting/enums/file-kind.js';
import type { PageRequest } from '../../../cross-cutting/paging/page.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListFilesQuery {
  context: RequestContext;
  /** null = todos os tipos que o usuário pode ver. */
  kind: FileKind | null;
  page: PageRequest;
}
