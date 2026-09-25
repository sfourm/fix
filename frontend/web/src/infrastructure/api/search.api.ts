import type { WidgetData, WidgetDefinition } from '@/domain/dashboard';
import type { Page } from '@/domain/page';
import type { DataSource, DataSourceInfo, FilterCriterion, SavedFilter, Visibility } from '@/domain/search';
import type { HttpClient } from '../http/http-client';

export interface SearchRequest {
  criteria: FilterCriterion[];
  filterId?: string | null;
  sort?: { field: string; direction: 'asc' | 'desc' } | null;
  page?: number;
  pageSize?: number;
}

export type SearchPage<T> = Page<T> & { truncated: boolean; cached: boolean };

export function createSearchApi(http: HttpClient) {
  return {
    sources: () => http.get<DataSourceInfo[]>('/search/sources'),
    /** Pesquisa num conjunto de dados; os itens têm o mesmo formato das listagens (Order, Mandate...). */
    run: <T>(source: DataSource, request: SearchRequest) => http.post<SearchPage<T>>(`/search/${source}`, request),
    /** Prévia de widget ainda não salvo. */
    preview: (widget: Pick<WidgetDefinition, 'type' | 'query'>) => http.post<WidgetData>('/search/aggregate', widget),
  };
}

export function createFilterApi(http: HttpClient) {
  return {
    list: (source?: DataSource) => http.get<SavedFilter[]>('/filters', { source }),
    create: (input: { name: string; source: DataSource; criteria: FilterCriterion[]; visibility: Visibility }) =>
      http.post<SavedFilter>('/filters', input),
    update: (id: string, input: { name: string; criteria: FilterCriterion[]; visibility: Visibility }) => http.put<SavedFilter>(`/filters/${id}`, input),
    remove: (id: string) => http.delete(`/filters/${id}`),
  };
}
