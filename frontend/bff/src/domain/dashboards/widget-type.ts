/**
 * Kpi: número em destaque · Column: barras verticais · Bar: barras horizontais (rótulos longos)
 * Line: evolução no tempo · Donut: composição de um todo · Table: valores agrupados em tabela.
 */
export const WIDGET_TYPES = ['Kpi', 'Column', 'Bar', 'Line', 'Donut', 'Table'] as const;

export type WidgetType = (typeof WIDGET_TYPES)[number];

/** Tipos que agrupam por uma dimensão; o Kpi mostra só o total. */
export const isGrouped = (type: WidgetType) => type !== 'Kpi';
