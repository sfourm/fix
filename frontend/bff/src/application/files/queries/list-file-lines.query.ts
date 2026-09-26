import type { FileLineStatus } from '../../../cross-cutting/enums/file-line-status.js';
import type { PageRequest } from '../../../cross-cutting/paging/page.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ListFileLinesQuery {
  context: RequestContext;
  fileId: string;
  /** null = todas. */
  status: FileLineStatus | null;
  page: PageRequest;
}
