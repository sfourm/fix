export const FILTER_OPERATORS = [
  'eq',
  'neq',
  'in',
  'notIn',
  'contains',
  'gt',
  'gte',
  'lt',
  'lte',
  'between',
  'isEmpty',
  'isNotEmpty',
] as const;

export type FilterOperator = (typeof FILTER_OPERATORS)[number];

/** Quantos valores cada operador espera: nenhum, um, uma lista ou um intervalo [de, até]. */
export type OperatorArity = 'none' | 'single' | 'list' | 'range';

export const operatorArity: Record<FilterOperator, OperatorArity> = {
  eq: 'single',
  neq: 'single',
  in: 'list',
  notIn: 'list',
  contains: 'single',
  gt: 'single',
  gte: 'single',
  lt: 'single',
  lte: 'single',
  between: 'range',
  isEmpty: 'none',
  isNotEmpty: 'none',
};
