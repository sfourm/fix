import type { DataSource } from '../common/data-source.js';
import { ensure } from '../common/domain-error.js';
import { createCriteria, type FilterCriterion } from '../filters/filter-criterion.js';
import { isGrouped, type WidgetType } from './widget-type.js';

export const AGGREGATIONS = ['count', 'sum', 'avg', 'min', 'max'] as const;
export type Aggregation = (typeof AGGREGATIONS)[number];

/** value-desc/value-asc: pelo valor medido · label: pela categoria (ordem cronológica em datas/meses). */
export const SORTS = ['value-desc', 'value-asc', 'label'] as const;
export type WidgetSort = (typeof SORTS)[number];

export const MAX_CATEGORIES = 24;

/** O que o widget mede: conjunto de dados, medida, dimensão de agrupamento e filtro (salvo e/ou condições próprias). */
export interface WidgetQuery {
  readonly source: DataSource;
  readonly aggregation: Aggregation;
  /** Campo numérico medido; null quando a medida é contagem. */
  readonly measureField: string | null;
  /** Dimensão de agrupamento; null no Kpi. */
  readonly groupBy: string | null;
  readonly filterId: string | null;
  readonly criteria: readonly FilterCriterion[];
  readonly sort: WidgetSort;
  /** Máximo de categorias exibidas; o restante é somado em "Outros". */
  readonly limit: number;
}

export function createQuery(type: WidgetType, query: WidgetQuery): WidgetQuery {
  if (query.aggregation === 'count') {
    ensure(query.measureField === null, 'A contagem não usa campo de medida.');
  } else {
    ensure(!!query.measureField, 'Soma, média, mínimo e máximo exigem um campo numérico.');
  }

  if (isGrouped(type)) {
    ensure(!!query.groupBy, 'Escolha a dimensão de agrupamento do gráfico.');
  } else {
    ensure(query.groupBy === null, 'O KPI mostra um único número: não agrupa por dimensão.');
  }

  ensure(Number.isInteger(query.limit) && query.limit >= 1 && query.limit <= MAX_CATEGORIES, `O limite de categorias deve ser de 1 a ${MAX_CATEGORIES}.`);
  if (type === 'Donut') {
    ensure(query.limit <= 8, 'A rosca comporta no máximo 8 fatias: use barras para mais categorias.');
  }

  return Object.freeze({ ...query, criteria: createCriteria(query.criteria) });
}
