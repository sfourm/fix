import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import { AppError } from '../../../cross-cutting/errors/app-error.js';
import type { Actor } from '../../../domain/common/actor.js';
import type { DataSource } from '../../../domain/common/data-source.js';
import type { WidgetQuery } from '../../../domain/dashboards/widget-query.js';
import { createQuery } from '../../../domain/dashboards/widget-query.js';
import type { WidgetType } from '../../../domain/dashboards/widget-type.js';
import { createCriteria, type FilterCriterion } from '../../../domain/filters/filter-criterion.js';
import type { SavedFilterRepository } from '../../../domain/filters/saved-filter.repository.js';
import type { ActorResolver } from '../../common/actor-resolver.js';
import { dataSourceCatalog, findField } from '../catalog/data-source-catalog.js';
import { operatorsByType } from '../catalog/field-definition.js';
import { aggregate } from '../engine/aggregator.js';
import { compileCriteria, validateCriteria } from '../engine/criteria-engine.js';
import type { SourceRowReader } from '../engine/source-row-reader.js';
import type { ListDataSourcesQuery } from '../queries/list-data-sources.query.js';
import type { SearchQuery } from '../queries/search.query.js';
import type { WidgetDataQuery } from '../queries/widget-data.query.js';
import type { DataSourceResponse } from '../responses/data-source.response.js';
import type { SearchPageResponse } from '../responses/search-page.response.js';
import type { WidgetDataResponse } from '../responses/widget-data.response.js';

/** Pesquisa e agregação sobre os conjuntos de dados do core, com filtros avulsos e salvos. */
export class SearchService {
  constructor(
    private readonly rows: SourceRowReader,
    private readonly filters: SavedFilterRepository,
    private readonly actors: ActorResolver,
  ) {}

  async listDataSources(query: ListDataSourcesQuery): Promise<DataSourceResponse[]> {
    await this.actors.resolve(query.context);
    return Object.values(dataSourceCatalog).map((source) => ({
      key: source.key,
      label: source.label,
      role: source.role,
      fields: source.fields.map((f) => ({
        key: f.key,
        label: f.label,
        type: f.type,
        format: f.format,
        options: f.options ?? [],
        groupable: f.groupable,
        measurable: f.measurable,
        operators: operatorsByType[f.type],
      })),
    }));
  }

  async search(query: SearchQuery): Promise<SearchPageResponse> {
    const actor = await this.actors.resolve(query.context);
    const criteria = await this.effectiveCriteria(actor, query.source, query.criteria, query.filterId);
    const { rows, truncated, cached } = await this.rows.read(query.context, query.source, criteria);

    let matched = rows.filter(compileCriteria(query.source, criteria));
    if (query.sort) {
      matched = SearchService.sorted(query.source, matched, query.sort);
    }

    const { page, pageSize } = query.page;
    return {
      items: matched.slice((page - 1) * pageSize, page * pageSize),
      page,
      pageSize,
      totalCount: matched.length,
      truncated,
      cached,
    };
  }

  /** Prévia do editor: o filtro salvo precisa ser visível ao usuário. */
  async widgetData(query: WidgetDataQuery): Promise<WidgetDataResponse> {
    const actor = await this.actors.resolve(query.context);
    return this.aggregate(query.context, actor, query.type, query.query);
  }

  /**
   * Calcula os dados de um widget. Em dashboards salvos o filtro já foi validado ao salvar
   * (dashboard público só usa filtro público), então quem vê o dashboard também pode usar o filtro.
   */
  async aggregate(
    context: RequestContext,
    actor: Actor,
    type: WidgetType,
    widgetQuery: WidgetQuery,
    options: { trustFilter?: boolean } = {},
  ): Promise<WidgetDataResponse> {
    const query = createQuery(type, widgetQuery);
    this.validateWidgetQuery(query);

    const criteria = await this.effectiveCriteria(actor, query.source, query.criteria, query.filterId, options.trustFilter);
    const { rows, truncated, cached } = await this.rows.read(context, query.source, criteria);
    const matched = rows.filter(compileCriteria(query.source, criteria));
    const result = aggregate(query.source, matched, query);

    const measureField = query.measureField ? findField(query.source, query.measureField) : null;
    const groupField = query.groupBy ? findField(query.source, query.groupBy) : null;

    return {
      aggregation: query.aggregation,
      measureLabel: measureField ? measureField.label : 'Quantidade',
      format: query.aggregation === 'count' ? 'integer' : measureField!.format,
      groupLabel: groupField?.label ?? null,
      total: result.total,
      points: result.points,
      rowCount: matched.length,
      truncated,
      cached,
      generatedAt: new Date().toISOString(),
    };
  }

  /** Campos da medida e da dimensão precisam existir e ter o papel certo no conjunto. */
  validateWidgetQuery(query: WidgetQuery): void {
    const fields: Record<string, string[]> = {};
    const label = dataSourceCatalog[query.source].label;

    if (query.measureField && !findField(query.source, query.measureField)?.measurable) {
      fields['query.measureField'] = [`"${query.measureField}" não é uma medida numérica de ${label}.`];
    }

    if (query.groupBy && !findField(query.source, query.groupBy)?.groupable) {
      fields['query.groupBy'] = [`"${query.groupBy}" não é uma dimensão de agrupamento de ${label}.`];
    }

    if (Object.keys(fields).length > 0) {
      throw AppError.validation('Widget inválido.', fields);
    }

    validateCriteria(query.source, query.criteria, 'query.criteria');
  }

  /** Condições avulsas + condições do filtro salvo (que precisa ser do mesmo conjunto de dados). */
  async effectiveCriteria(
    actor: Actor,
    source: DataSource,
    criteria: readonly FilterCriterion[],
    filterId: string | null,
    trustFilter = false,
  ): Promise<FilterCriterion[]> {
    const own = createCriteria(criteria);
    validateCriteria(source, own);

    if (!filterId) {
      return own;
    }

    const filter = await this.filters.findById(actor.organizationId, filterId);
    if (!filter) {
      throw new AppError('not_found', 'Filtro salvo não encontrado.');
    }

    if (!trustFilter) {
      filter.ensureCanView(actor);
    }

    if (filter.source !== source) {
      throw AppError.validation(`O filtro "${filter.name}" é de ${dataSourceCatalog[filter.source].label}, não de ${dataSourceCatalog[source].label}.`);
    }

    return [...filter.criteria, ...own];
  }

  private static sorted(source: DataSource, rows: unknown[], sort: { field: string; direction: 'asc' | 'desc' }): unknown[] {
    const field = findField(source, sort.field);
    if (!field) {
      throw AppError.validation(`Não é possível ordenar por "${sort.field}".`);
    }

    const factor = sort.direction === 'asc' ? 1 : -1;
    return [...rows].sort((a, b) => {
      const x = field.get(a);
      const y = field.get(b);
      if (x === y) return 0;
      if (x === null) return 1;
      if (y === null) return -1;
      return (typeof x === 'string' && typeof y === 'string' ? x.localeCompare(y, 'pt-BR') : x < y ? -1 : 1) * factor;
    });
  }
}
