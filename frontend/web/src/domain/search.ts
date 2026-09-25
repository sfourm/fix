export const DATA_SOURCES = ['orders', 'mandates', 'counterparties', 'policies'] as const;
export type DataSource = (typeof DATA_SOURCES)[number];

export const VISIBILITIES = ['Private', 'Public'] as const;
export type Visibility = (typeof VISIBILITIES)[number];

export type FilterOperator =
  | 'eq'
  | 'neq'
  | 'in'
  | 'notIn'
  | 'contains'
  | 'gt'
  | 'gte'
  | 'lt'
  | 'lte'
  | 'between'
  | 'isEmpty'
  | 'isNotEmpty';

export type Scalar = string | number | boolean;

export interface FilterCriterion {
  field: string;
  operator: FilterOperator;
  value: Scalar | Scalar[] | null;
}

export type FieldType = 'text' | 'number' | 'date' | 'boolean' | 'enum' | 'id';
export type ValueFormat = 'integer' | 'decimal' | 'usd' | 'percent' | 'text' | 'date' | 'month';

/** Campo de um conjunto de dados, como o BFF descreve no catálogo (/api/search/sources). */
export interface DataSourceField {
  key: string;
  label: string;
  type: FieldType;
  format: ValueFormat;
  options: { value: string; label: string }[];
  groupable: boolean;
  measurable: boolean;
  operators: FilterOperator[];
}

export interface DataSourceInfo {
  key: DataSource;
  label: string;
  /** Role necessária para ler o conjunto. */
  role: string;
  fields: DataSourceField[];
}

export interface SavedFilter {
  id: string;
  name: string;
  source: DataSource;
  criteria: FilterCriterion[];
  visibility: Visibility;
  ownerId: string;
  isMine: boolean;
  canEdit: boolean;
  updatedAt: string;
}

export const operatorLabel: Record<FilterOperator, string> = {
  eq: 'é igual a',
  neq: 'é diferente de',
  in: 'é um de',
  notIn: 'não é um de',
  contains: 'contém',
  gt: 'maior que',
  gte: 'maior ou igual a',
  lt: 'menor que',
  lte: 'menor ou igual a',
  between: 'entre',
  isEmpty: 'está vazio',
  isNotEmpty: 'está preenchido',
};

export const visibilityLabel: Record<Visibility, string> = { Private: 'Privado', Public: 'Público na organização' };
