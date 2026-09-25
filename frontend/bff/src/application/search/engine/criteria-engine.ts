import type { DataSource } from '../../../domain/common/data-source.js';
import type { FilterCriterion, Scalar } from '../../../domain/filters/filter-criterion.js';
import { AppError } from '../../../cross-cutting/errors/app-error.js';
import { dataSourceCatalog, findField } from '../catalog/data-source-catalog.js';
import { operatorsByType, type FieldDefinition } from '../catalog/field-definition.js';

/** Garante que cada condição usa um campo do conjunto de dados, com operador e valores compatíveis com o tipo. */
export function validateCriteria(source: DataSource, criteria: readonly FilterCriterion[], path = 'criteria'): void {
  const fields: Record<string, string[]> = {};
  const fail = (index: number, message: string) => (fields[`${path}.${index}`] ??= []).push(message);

  criteria.forEach((criterion, index) => {
    const field = findField(source, criterion.field);
    if (!field) {
      fail(index, `O campo "${criterion.field}" não existe em ${dataSourceCatalog[source].label}.`);
      return;
    }

    if (!operatorsByType[field.type].includes(criterion.operator)) {
      fail(index, `O operador "${criterion.operator}" não se aplica a ${field.label}.`);
      return;
    }

    const values = Array.isArray(criterion.value) ? criterion.value : criterion.value === null ? [] : [criterion.value];
    const invalid = values.find((value) => !acceptsValue(field, value));
    if (invalid !== undefined) {
      fail(index, `Valor inválido para ${field.label}: ${String(invalid)}.`);
    }
  });

  if (Object.keys(fields).length > 0) {
    throw AppError.validation('Filtro inválido.', fields);
  }
}

function acceptsValue(field: FieldDefinition<unknown>, value: Scalar): boolean {
  switch (field.type) {
    case 'number':
      return typeof value === 'number' && Number.isFinite(value);
    case 'boolean':
      return typeof value === 'boolean';
    case 'date':
      return typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value);
    case 'enum':
      return typeof value === 'string' && !!field.options?.some((o) => o.value === value);
    default:
      return typeof value === 'string';
  }
}

/** Monta o predicado que aplica todas as condições (E lógico) a uma linha do conjunto. */
export function compileCriteria<TRow>(source: DataSource, criteria: readonly FilterCriterion[]): (row: TRow) => boolean {
  const tests = criteria.map((criterion) => {
    const field = findField(source, criterion.field)! as FieldDefinition<TRow>;
    const normalize = (value: Scalar | null) => (field.type === 'text' && typeof value === 'string' ? value.toLocaleLowerCase('pt-BR') : value);
    const expected = Array.isArray(criterion.value) ? criterion.value.map(normalize) : normalize(criterion.value as Scalar | null);

    return (row: TRow): boolean => {
      const actual = normalize(field.get(row));
      const empty = actual === null || actual === '';

      switch (criterion.operator) {
        case 'isEmpty':
          return empty;
        case 'isNotEmpty':
          return !empty;
        case 'eq':
          return actual === expected;
        case 'neq':
          return actual !== expected;
        case 'in':
          return (expected as Scalar[]).includes(actual as Scalar);
        case 'notIn':
          return !(expected as Scalar[]).includes(actual as Scalar);
        case 'contains':
          return typeof actual === 'string' && actual.includes(String(expected));
        case 'gt':
          return !empty && (actual as Scalar) > (expected as Scalar);
        case 'gte':
          return !empty && (actual as Scalar) >= (expected as Scalar);
        case 'lt':
          return !empty && (actual as Scalar) < (expected as Scalar);
        case 'lte':
          return !empty && (actual as Scalar) <= (expected as Scalar);
        case 'between': {
          const [from, to] = expected as Scalar[];
          return !empty && (actual as Scalar) >= from! && (actual as Scalar) <= to!;
        }
      }
    };
  });

  return (row) => tests.every((test) => test(row));
}
