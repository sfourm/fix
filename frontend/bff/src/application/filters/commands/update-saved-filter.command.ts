import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Visibility } from '../../../domain/common/visibility.js';
import type { FilterCriterion } from '../../../domain/filters/filter-criterion.js';

export interface UpdateSavedFilterCommand {
  context: RequestContext;
  id: string;
  name: string;
  criteria: FilterCriterion[];
  visibility: Visibility;
}
