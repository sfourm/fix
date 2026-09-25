import type { WidgetDefinition } from '../../../domain/dashboards/dashboard-widget.js';
import type { WidgetQuery } from '../../../domain/dashboards/widget-query.js';
import type { WidgetType } from '../../../domain/dashboards/widget-type.js';

export const DASHBOARD_TEMPLATES = ['blank', 'fix-overview'] as const;
export type DashboardTemplate = (typeof DASHBOARD_TEMPLATES)[number];

const query = (q: Partial<WidgetQuery> & Pick<WidgetQuery, 'source'>): WidgetQuery => ({
  aggregation: 'count',
  measureField: null,
  groupBy: null,
  filterId: null,
  criteria: [],
  sort: 'value-desc',
  limit: 8,
  ...q,
});

const widget = (title: string, type: WidgetType, width: number, height: number, q: WidgetQuery, mode: 'single' | 'category' = 'single'): WidgetDefinition => ({
  title,
  type,
  layout: { width, height },
  query: q,
  colors: { mode, palette: [] },
});

/** Visão geral do processo FIX: autorizações, execução e confirmations. */
const fixOverview: WidgetDefinition[] = [
  widget('Mandatos ativos', 'Kpi', 3, 140, query({ source: 'mandates', criteria: [{ field: 'status', operator: 'eq', value: 'Active' }] })),
  widget('Boletas aguardando aprovação', 'Kpi', 3, 140, query({ source: 'orders', criteria: [{ field: 'approval', operator: 'eq', value: 'PendingApproval' }] })),
  widget(
    'Confirmations em aberto',
    'Kpi',
    3,
    140,
    query({
      source: 'orders',
      criteria: [
        { field: 'approval', operator: 'eq', value: 'Approved' },
        { field: 'confirmation', operator: 'in', value: ['Pending', 'Divergent', 'Refused'] },
      ],
    }),
  ),
  widget('Lotes aprovados', 'Kpi', 3, 140, query({ source: 'orders', aggregation: 'sum', measureField: 'lots', criteria: [{ field: 'approval', operator: 'eq', value: 'Approved' }] })),
  widget('Lotes negociados por mês', 'Line', 8, 300, query({ source: 'orders', aggregation: 'sum', measureField: 'lots', groupBy: 'tradeMonth', sort: 'label', limit: 12 })),
  widget('Boletas por instrumento', 'Donut', 4, 300, query({ source: 'orders', groupBy: 'type', limit: 3 }), 'category'),
  widget(
    'Saldo dos mandatos ativos',
    'Bar',
    6,
    300,
    query({ source: 'mandates', aggregation: 'sum', measureField: 'balance', groupBy: 'title', criteria: [{ field: 'status', operator: 'eq', value: 'Active' }] }),
  ),
  widget('Boletas por contraparte', 'Table', 6, 300, query({ source: 'orders', groupBy: 'counterpartyName', limit: 10 })),
];

export const dashboardTemplates: Record<DashboardTemplate, WidgetDefinition[]> = {
  blank: [],
  'fix-overview': fixOverview,
};
