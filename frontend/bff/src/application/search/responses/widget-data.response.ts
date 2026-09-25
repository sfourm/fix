import type { Aggregation } from '../../../domain/dashboards/widget-query.js';
import type { ValueFormat } from '../catalog/field-definition.js';

export interface WidgetPointResponse {
  key: string;
  label: string;
  value: number | null;
  /** Quantas linhas caíram na categoria. */
  count: number;
}

export interface WidgetDataResponse {
  aggregation: Aggregation;
  measureLabel: string;
  format: ValueFormat;
  groupLabel: string | null;
  total: number | null;
  points: WidgetPointResponse[];
  rowCount: number;
  truncated: boolean;
  cached: boolean;
  generatedAt: string;
}
