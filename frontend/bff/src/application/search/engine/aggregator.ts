import type { DataSource } from '../../../domain/common/data-source.js';
import type { Scalar } from '../../../domain/filters/filter-criterion.js';
import type { Aggregation, WidgetSort } from '../../../domain/dashboards/widget-query.js';
import { findField } from '../catalog/data-source-catalog.js';
import type { FieldDefinition } from '../catalog/field-definition.js';

export const EMPTY_KEY = '__empty__';
export const OTHERS_KEY = '__others__';

export interface AggregatePoint {
  key: string;
  label: string;
  value: number | null;
  count: number;
}

export interface AggregateResult {
  /** Medida sobre todas as linhas (o número do KPI). */
  total: number | null;
  points: AggregatePoint[];
}

export interface AggregateSpec {
  aggregation: Aggregation;
  measureField: string | null;
  groupBy: string | null;
  sort: WidgetSort;
  limit: number;
}

const MONTHS = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

/** Agrega as linhas: total geral e, se houver dimensão, um ponto por categoria (excedentes somados em "Outros"). */
export function aggregate<TRow>(source: DataSource, rows: TRow[], spec: AggregateSpec): AggregateResult {
  const measureField = spec.measureField ? (findField(source, spec.measureField) as FieldDefinition<TRow>) : null;
  const measureOf = (group: TRow[]) => measure(group, spec.aggregation, measureField);
  const total = measureOf(rows);

  if (!spec.groupBy) {
    return { total, points: [] };
  }

  const dimension = findField(source, spec.groupBy) as FieldDefinition<TRow>;
  const groups = new Map<string, TRow[]>();
  for (const row of rows) {
    const key = keyOf(dimension.get(row));
    const group = groups.get(key);
    if (group) group.push(row);
    else groups.set(key, [row]);
  }

  const points = [...groups.entries()].map(([key, group]) => ({
    key,
    label: labelOf(dimension, key),
    value: measureOf(group),
    count: group.length,
    rows: group,
  }));

  const byValue = (a: { value: number | null }, b: { value: number | null }) => (a.value ?? -Infinity) - (b.value ?? -Infinity);
  if (spec.sort === 'label') points.sort((a, b) => (a.key === EMPTY_KEY ? 1 : b.key === EMPTY_KEY ? -1 : a.key.localeCompare(b.key, 'pt-BR')));
  else if (spec.sort === 'value-asc') points.sort(byValue);
  else points.sort((a, b) => byValue(b, a));

  const strip = ({ rows: _, ...point }: (typeof points)[number]): AggregatePoint => point;

  if (points.length <= spec.limit) {
    return { total, points: points.map(strip) };
  }

  // Ordem por rótulo (séries no tempo): mantém as categorias mais recentes, sem "Outros".
  if (spec.sort === 'label') {
    return { total, points: points.slice(-spec.limit).map(strip) };
  }

  const kept = points.slice(0, spec.limit - 1);
  const rest = points.slice(spec.limit - 1);
  const restRows = rest.flatMap((p) => p.rows);
  const others: AggregatePoint = {
    key: OTHERS_KEY,
    label: `Outros (${rest.length})`,
    value: measureOf(restRows),
    count: restRows.length,
  };

  return { total, points: [...kept.map(strip), others] };
}

function measure<TRow>(rows: TRow[], aggregation: Aggregation, field: FieldDefinition<TRow> | null): number | null {
  if (aggregation === 'count' || !field) {
    return rows.length;
  }

  const values = rows.map((row) => field.get(row)).filter((v): v is number => typeof v === 'number');
  if (values.length === 0) {
    return null;
  }

  switch (aggregation) {
    case 'sum':
      return values.reduce((a, b) => a + b, 0);
    case 'avg':
      return values.reduce((a, b) => a + b, 0) / values.length;
    case 'min':
      return Math.min(...values);
    case 'max':
      return Math.max(...values);
  }
}

const keyOf = (value: Scalar | null): string => (value === null || value === '' ? EMPTY_KEY : String(value));

function labelOf(field: FieldDefinition<unknown>, key: string): string {
  if (key === EMPTY_KEY) return '(vazio)';
  if (field.type === 'boolean') return key === 'true' ? 'Sim' : 'Não';
  if (field.options) return field.options.find((o) => o.value === key)?.label ?? key;
  if (field.format === 'month') {
    const [year, monthIndex] = key.split('-');
    return `${MONTHS[Number(monthIndex) - 1] ?? monthIndex}/${year}`;
  }

  return key;
}
