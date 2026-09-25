import { z } from 'zod';
import { DASHBOARD_TEMPLATES } from '../../application/dashboards/templates/dashboard-templates.js';
import { DATA_SOURCES } from '../../domain/common/data-source.js';
import { VISIBILITIES } from '../../domain/common/visibility.js';
import { COLOR_MODES, MAX_COLORS } from '../../domain/dashboards/widget-colors.js';
import { AGGREGATIONS, SORTS } from '../../domain/dashboards/widget-query.js';
import { WIDGET_TYPES } from '../../domain/dashboards/widget-type.js';
import { FILTER_OPERATORS } from '../../domain/filters/filter-operator.js';
import { MAX_CRITERIA } from '../../domain/filters/filter-criterion.js';
import { pageQuery } from './validation.js';

/**
 * Forma dos payloads de dashboards, filtros e pesquisa. As regras de negócio (tamanhos, combinações de
 * medida/dimensão, visibilidade) ficam no domínio do BFF; aqui só se garante tipo e formato.
 */
const scalar = z.union([z.string().max(200), z.number().finite(), z.boolean()]);

export const criterionSchema = z.object({
  field: z.string().trim().min(1).max(64),
  operator: z.enum(FILTER_OPERATORS),
  value: z.union([scalar, z.array(scalar).max(50)]).nullish().transform((v) => v ?? null),
});

const criteria = z.array(criterionSchema).max(MAX_CRITERIA).default([]);
const name = z.string().trim().min(1).max(120);
const nullableId = z.guid().nullish().transform((v) => v ?? null);

const widgetQuery = z.object({
  source: z.enum(DATA_SOURCES),
  aggregation: z.enum(AGGREGATIONS).default('count'),
  measureField: z.string().max(64).nullish().transform((v) => v || null),
  groupBy: z.string().max(64).nullish().transform((v) => v || null),
  filterId: nullableId,
  criteria,
  sort: z.enum(SORTS).default('value-desc'),
  limit: z.number().int().default(8),
});

export const widgetSchema = z.object({
  title: z.string().max(80),
  type: z.enum(WIDGET_TYPES),
  layout: z.object({ width: z.number().int(), height: z.number().int() }),
  query: widgetQuery,
  colors: z
    .object({
      mode: z.enum(COLOR_MODES).default('single'),
      palette: z.array(z.string()).max(MAX_COLORS).default([]),
    })
    .default({ mode: 'single', palette: [] }),
});

export const dashboardSchemas = {
  createDashboard: z.object({
    name,
    description: z.string().max(500).nullish().transform((v) => v ?? null),
    visibility: z.enum(VISIBILITIES).default('Private'),
    template: z.enum(DASHBOARD_TEMPLATES).default('blank'),
  }),
  updateDashboard: z.object({
    name,
    description: z.string().max(500).nullish().transform((v) => v ?? null),
    visibility: z.enum(VISIBILITIES),
  }),
  duplicateDashboard: z.object({ name: name.nullish().transform((v) => v ?? null) }),
  widget: widgetSchema,
  reorderWidgets: z.object({ widgetIds: z.array(z.guid()).max(50) }),
  widgetParams: z.object({ id: z.guid(), widgetId: z.guid() }),

  savedFilter: z.object({ name, source: z.enum(DATA_SOURCES), criteria, visibility: z.enum(VISIBILITIES).default('Private') }),
  updateSavedFilter: z.object({ name, criteria, visibility: z.enum(VISIBILITIES) }),
  savedFiltersQuery: z.object({ source: z.enum(DATA_SOURCES).optional() }),

  search: pageQuery.extend({
    criteria,
    filterId: nullableId,
    sort: z.object({ field: z.string().max(64), direction: z.enum(['asc', 'desc']) }).nullish().transform((v) => v ?? null),
  }),
  sourceParam: z.object({ source: z.enum(DATA_SOURCES) }),
  widgetPreview: widgetSchema.pick({ type: true, query: true }),
};
