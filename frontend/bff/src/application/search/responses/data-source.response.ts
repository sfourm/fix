import type { FilterOperator } from '../../../domain/filters/filter-operator.js';
import type { FieldOption, FieldType, ValueFormat } from '../catalog/field-definition.js';

export interface DataSourceFieldResponse {
  key: string;
  label: string;
  type: FieldType;
  format: ValueFormat;
  options: FieldOption[];
  groupable: boolean;
  measurable: boolean;
  operators: FilterOperator[];
}

/** Catálogo que o web usa para montar filtros e widgets sem conhecer os campos de antemão. */
export interface DataSourceResponse {
  key: string;
  label: string;
  role: string;
  fields: DataSourceFieldResponse[];
}
