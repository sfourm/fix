import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { PageRequest } from '../../../cross-cutting/paging/page.js';
import type { DataSource } from '../../../domain/common/data-source.js';
import type { FilterCriterion } from '../../../domain/filters/filter-criterion.js';

export interface SearchQuery {
  context: RequestContext;
  source: DataSource;
  /** Condições avulsas, somadas às do filtro salvo (E lógico). */
  criteria: FilterCriterion[];
  filterId: string | null;
  sort: { field: string; direction: 'asc' | 'desc' } | null;
  page: PageRequest;
}
