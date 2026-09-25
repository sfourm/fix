import { ensure } from '../common/domain-error.js';
import { operatorArity, type FilterOperator } from './filter-operator.js';

export type Scalar = string | number | boolean;
export type CriterionValue = Scalar | Scalar[] | null;

/** Condição de filtro: campo do conjunto de dados, operador e valor(es). Imutável. */
export interface FilterCriterion {
  readonly field: string;
  readonly operator: FilterOperator;
  readonly value: CriterionValue;
}

export const MAX_CRITERIA = 20;

const isScalar = (value: unknown): value is Scalar => ['string', 'number', 'boolean'].includes(typeof value);

/** Valida a forma do critério pelo operador; se o campo existe no conjunto de dados é checado na application. */
export function createCriterion(field: string, operator: FilterOperator, value: CriterionValue): FilterCriterion {
  ensure(/^[a-zA-Z][a-zA-Z0-9.]{0,63}$/.test(field), `Campo de filtro inválido: "${field}".`);

  switch (operatorArity[operator]) {
    case 'none':
      return Object.freeze({ field, operator, value: null });
    case 'single':
      ensure(isScalar(value) && value !== '', `O filtro "${field}" (${operator}) exige um valor.`);
      return Object.freeze({ field, operator, value });
    case 'list':
      ensure(Array.isArray(value) && value.length > 0 && value.length <= 50 && value.every(isScalar), `O filtro "${field}" (${operator}) exige uma lista de 1 a 50 valores.`);
      return Object.freeze({ field, operator, value: Object.freeze([...value]) as Scalar[] });
    case 'range': {
      ensure(Array.isArray(value) && value.length === 2 && value.every(isScalar), `O filtro "${field}" (entre) exige dois valores: de e até.`);
      const [from, to] = value as [Scalar, Scalar];
      ensure(from <= to, `O filtro "${field}" (entre): o início não pode ser maior que o fim.`);
      return Object.freeze({ field, operator, value: Object.freeze([from, to]) as Scalar[] });
    }
  }
}

export function createCriteria(criteria: readonly FilterCriterion[]): FilterCriterion[] {
  ensure(criteria.length <= MAX_CRITERIA, `No máximo ${MAX_CRITERIA} condições por filtro.`);
  return criteria.map((c) => createCriterion(c.field, c.operator, c.value));
}
