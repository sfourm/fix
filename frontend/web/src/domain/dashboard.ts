import type { DataSource, FilterCriterion, ValueFormat, Visibility } from './search';

export const WIDGET_TYPES = ['Kpi', 'Column', 'Bar', 'Line', 'Donut', 'Table'] as const;
export type WidgetType = (typeof WIDGET_TYPES)[number];

export const AGGREGATIONS = ['count', 'sum', 'avg', 'min', 'max'] as const;
export type Aggregation = (typeof AGGREGATIONS)[number];

export type WidgetSort = 'value-desc' | 'value-asc' | 'label';
export type ColorMode = 'single' | 'category';

export interface WidgetQuery {
  source: DataSource;
  aggregation: Aggregation;
  measureField: string | null;
  groupBy: string | null;
  filterId: string | null;
  criteria: FilterCriterion[];
  sort: WidgetSort;
  limit: number;
}

/** Definição editável de um widget: tipo, tamanho na grade, dados e cores. */
export interface WidgetDefinition {
  title: string;
  type: WidgetType;
  /** width: colunas (2–12) · height: pixels (120–720). */
  layout: { width: number; height: number };
  query: WidgetQuery;
  colors: { mode: ColorMode; palette: string[] };
}

export interface Widget extends WidgetDefinition {
  id: string;
}

export interface DashboardSummary {
  id: string;
  name: string;
  description: string | null;
  visibility: Visibility;
  ownerId: string;
  isMine: boolean;
  canEdit: boolean;
  widgetCount: number;
  updatedAt: string;
}

export interface Dashboard extends DashboardSummary {
  widgets: Widget[];
}

export interface WidgetPoint {
  key: string;
  label: string;
  value: number | null;
  count: number;
}

export interface WidgetData {
  aggregation: Aggregation;
  measureLabel: string;
  format: ValueFormat;
  groupLabel: string | null;
  total: number | null;
  points: WidgetPoint[];
  rowCount: number;
  truncated: boolean;
  cached: boolean;
  generatedAt: string;
}

export const GRID_COLUMNS = 12;
export const MIN_WIDGET_HEIGHT = 120;
export const MAX_WIDGET_HEIGHT = 720;

export const widgetTypeLabel: Record<WidgetType, string> = {
  Kpi: 'Número (KPI)',
  Column: 'Colunas',
  Bar: 'Barras horizontais',
  Line: 'Linha (evolução)',
  Donut: 'Rosca (composição)',
  Table: 'Tabela',
};

export const aggregationLabel: Record<Aggregation, string> = {
  count: 'Quantidade',
  sum: 'Soma',
  avg: 'Média',
  min: 'Mínimo',
  max: 'Máximo',
};

export const OTHERS_KEY = '__others__';
