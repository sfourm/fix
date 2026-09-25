import type { DataSource } from '../../../domain/common/data-source.js';
import type { Scalar } from '../../../domain/filters/filter-criterion.js';
import type { FilterOperator } from '../../../domain/filters/filter-operator.js';

export type FieldType = 'text' | 'number' | 'date' | 'boolean' | 'enum' | 'id';

/** Como o web formata valores do campo (e das medidas calculadas sobre ele). */
export type ValueFormat = 'integer' | 'decimal' | 'usd' | 'percent' | 'text' | 'date' | 'month';

export interface FieldOption {
  value: string;
  label: string;
}

export interface FieldDefinition<TRow> {
  key: string;
  label: string;
  type: FieldType;
  format: ValueFormat;
  options?: FieldOption[];
  /** Pode ser dimensão de agrupamento em gráficos. */
  groupable: boolean;
  /** Pode ser medido (soma, média, mínimo, máximo). */
  measurable: boolean;
  get(row: TRow): Scalar | null;
}

export interface DataSourceDefinition<TRow = unknown> {
  key: DataSource;
  label: string;
  /** Role exigida pelo core para ler o conjunto (o web esconde o que o usuário não pode ver). */
  role: string;
  fields: FieldDefinition<TRow>[];
}

export const operatorsByType: Record<FieldType, FilterOperator[]> = {
  text: ['eq', 'neq', 'contains', 'in', 'isEmpty', 'isNotEmpty'],
  enum: ['eq', 'neq', 'in', 'notIn'],
  number: ['eq', 'neq', 'gt', 'gte', 'lt', 'lte', 'between', 'isEmpty', 'isNotEmpty'],
  date: ['eq', 'gt', 'gte', 'lt', 'lte', 'between', 'isEmpty', 'isNotEmpty'],
  boolean: ['eq'],
  id: ['eq', 'neq', 'in'],
};
