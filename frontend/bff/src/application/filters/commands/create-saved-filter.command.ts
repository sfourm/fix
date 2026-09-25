import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { DataSource } from '../../../domain/common/data-source.js';
import type { Visibility } from '../../../domain/common/visibility.js';
import type { FilterCriterion } from '../../../domain/filters/filter-criterion.js';

export interface CreateSavedFilterCommand {
  context: RequestContext;
  name: string;
  source: DataSource;
  criteria: FilterCriterion[];
  visibility: Visibility;
}
