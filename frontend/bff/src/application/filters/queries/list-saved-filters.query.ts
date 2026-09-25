import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { DataSource } from '../../../domain/common/data-source.js';

export interface ListSavedFiltersQuery {
  context: RequestContext;
  source: DataSource | null;
}
