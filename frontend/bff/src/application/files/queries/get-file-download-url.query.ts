import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface GetFileDownloadUrlQuery {
  context: RequestContext;
  id: string;
}
