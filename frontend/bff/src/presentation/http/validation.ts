import { z } from 'zod';
import { APPROVAL_STATUSES } from '../../cross-cutting/enums/approval-status.js';
import { COMMODITIES } from '../../cross-cutting/enums/commodity.js';
import { CONFIRMATION_STATUSES } from '../../cross-cutting/enums/confirmation-status.js';
import { COUNTERPARTY_TYPES } from '../../cross-cutting/enums/counterparty-type.js';
import { DESKS } from '../../cross-cutting/enums/desk.js';
import { FILE_KINDS } from '../../cross-cutting/enums/file-kind.js';
import { FILE_LINE_STATUSES } from '../../cross-cutting/enums/file-line-status.js';
import { INSTRUMENT_PERMISSIONS } from '../../cross-cutting/enums/instrument-permission.js';
import { MANDATE_STATUSES } from '../../cross-cutting/enums/mandate-status.js';
import { MANDATE_TYPES } from '../../cross-cutting/enums/mandate-type.js';
import { MEASUREMENT_UNITS } from '../../cross-cutting/enums/measurement-unit.js';
import { OPTION_KINDS } from '../../cross-cutting/enums/option-kind.js';
import { ORDER_TYPES } from '../../cross-cutting/enums/order-type.js';
import { RISK_FACTORS } from '../../cross-cutting/enums/risk-factor.js';
import { SECTORS } from '../../cross-cutting/enums/sector.js';
import { TRADE_DIRECTIONS } from '../../cross-cutting/enums/trade-direction.js';
import { AppError, type FieldErrors } from '../../cross-cutting/errors/app-error.js';

z.config(z.locales.pt());

/** Valida a entrada da requisição; erros viram 400 com os campos inválidos. */
export function parse<T extends z.ZodType>(schema: T, data: unknown): z.infer<T> {
  const result = schema.safeParse(data);
  if (result.success) {
    return result.data;
  }

  const fields: FieldErrors = {};
  for (const issue of result.error.issues) {
    const key = issue.path.join('.') || '_';
    (fields[key] ??= []).push(issue.message);
  }

  throw AppError.validation('Dados inválidos.', fields);
}

// ---------- primitivas ----------

const text = (max: number) => z.string().trim().min(1).max(max);
const optionalText = (max: number) =>
  z
    .string()
    .trim()
    .max(max)
    .nullish()
    .transform((value) => (value ? value : null));
const date = z.iso.date();
const optionalDate = date.nullish().transform((value) => value ?? null);
const optionalNumber = z.number().finite().nullish().transform((value) => value ?? null);
const optionalPositive = z.number().positive().nullish().transform((value) => value ?? null);
const pct = z.number().min(0).max(100);
const optionalPct = pct.nullish().transform((value) => value ?? null);
const nullableEnum = <T extends readonly [string, ...string[]]>(values: T) =>
  z.enum(values).nullish().transform((value) => value ?? null);
/** Ano-safra no formato AA/AA (ex.: 26/27). */
const crop = z.string().trim().regex(/^\d{2}\/\d{2}$/, 'Use o formato AA/AA.');

export const idParam = z.object({ id: z.guid() });
export const childParam = z.object({ id: z.guid(), childId: z.guid() });

export const pageQuery = z.object({
  page: z.coerce.number().int().min(1).default(1),
  pageSize: z.coerce.number().int().min(1).max(100).default(20),
});

const toPage = <T extends { page: number; pageSize: number }>({ page, pageSize, ...rest }: T) => ({
  ...rest,
  page: { page, pageSize },
});

const note = z.object({ note: optionalText(1000) });

/** Filtros da auditoria (todos opcionais). Período em ISO 8601 com fuso. */
const timelineFilters = z.object({
  entityType: z.string().trim().max(128).optional(),
  entityId: z.guid().optional(),
  action: z.enum(['Created', 'Updated', 'Deleted']).optional(),
  authorId: z.guid().optional(),
  from: z.iso.datetime({ offset: true }).optional(),
  to: z.iso.datetime({ offset: true }).optional(),
  search: z.string().trim().min(1).max(200).optional(),
});
const requiredNote = z.object({ note: text(1000) });

// ---------- políticas ----------

// Tetos sobre referência (teto absoluto, câmbio, frete) podem passar de 100%, como no domínio.
const extendedPct = z.number().min(0).max(300);

const policyLimits = z
  .object({
    hedgeHorizonYears: z.number().int().min(1).max(10),
    absoluteCeilingPct: extendedPct,
    fxFixedMinPct: extendedPct,
    fxFixedMaxPct: extendedPct,
    fxUnfixedMaxPct: pct,
    marginCashMaxPct: pct,
    physicalConcentrationMaxPct: pct,
    financialConcentrationMaxPct: pct,
    logisticsDeadlineMonths: z.number().int().min(0).max(36),
    freightCeilingPct: extendedPct,
    coveredCallMaxPct: pct,
    contingency1MonthPct: pct,
    contingency6MonthsPct: pct,
    contingency12MonthsPct: pct,
    contingency24MonthsPct: pct,
    contingency36MonthsPct: pct,
    buybackTriggerPct: extendedPct,
    buybackDeadlineBusinessDays: z.number().int().min(1).max(60),
    stressSigmas: z.number().positive().max(10),
    stressDays: z.number().int().min(1).max(60),
    pricingHotPercentile: z.number().int().min(0).max(100),
    pricingColdPercentile: z.number().int().min(0).max(100),
    mixShiftMaxPp: pct,
    confirmationDeadlineBusinessDays: z.number().int().min(0).max(30),
    registrationDeadlineDays: z.number().int().min(0).max(30),
    deviationReportHours: z.number().int().min(1).max(720),
  })
  .refine(
    (l) =>
      l.contingency1MonthPct <= l.contingency6MonthsPct &&
      l.contingency6MonthsPct <= l.contingency12MonthsPct &&
      l.contingency12MonthsPct <= l.contingency24MonthsPct &&
      l.contingency24MonthsPct <= l.contingency36MonthsPct,
    { path: ['contingency36MonthsPct'], message: 'A contingência não pode diminuir com o prazo (1 ≤ 6 ≤ 12 ≤ 24 ≤ 36 meses).' },
  )
  .refine((l) => l.pricingColdPercentile < l.pricingHotPercentile, {
    path: ['pricingHotPercentile'],
    message: 'O percentil frio deve ser menor que o quente.',
  })
  .refine((l) => l.fxFixedMaxPct >= l.fxFixedMinPct, {
    path: ['fxFixedMaxPct'],
    message: 'O máximo da banda de câmbio não pode ser menor que o mínimo.',
  });

const policyAxis = z.object({
  code: text(16),
  title: text(200),
  factor: z.enum(RISK_FACTORS),
  statement: optionalText(2000),
  limitDescription: optionalText(1000),
  approver: optionalText(200),
  restrictions: z.array(text(500)).default([]),
});

const coverageBand = z
  .object({
    horizon: text(64),
    crop,
    minPct: pct,
    maxPct: pct,
    note: optionalText(500),
  })
  .refine((b) => b.maxPct >= b.minPct, { path: ['maxPct'], message: 'O máximo não pode ser menor que o mínimo.' });

const policyInstrument = z.object({
  name: text(200),
  permission: z.enum(INSTRUMENT_PERMISSIONS),
  condition: optionalText(500),
});

const policyHeader = {
  code: text(32),
  title: text(200),
  description: optionalText(2000),
  validFrom: date,
  validTo: optionalDate,
};

const validRange = <T extends { validFrom: string; validTo: string | null }>(p: T) => !p.validTo || p.validTo >= p.validFrom;
const rangeMessage = { path: ['validTo'], message: 'O fim da vigência não pode ser anterior ao início.' };

// ---------- mandatos ----------

const mandateTerms = z
  .object({
    title: text(200),
    criteria: optionalText(1000),
    commodity: nullableEnum(COMMODITIES),
    tenor: optionalText(16),
    quantity: optionalPositive,
    quantityUnit: z.enum(MEASUREMENT_UNITS),
    price: z.object({
      atMarket: z.boolean().default(false),
      target: optionalNumber,
      min: optionalNumber,
      max: optionalNumber,
      unit: optionalText(16),
    }),
    windowStart: optionalDate,
    windowEnd: optionalDate,
  })
  .refine((t) => !t.windowStart || !t.windowEnd || t.windowEnd >= t.windowStart, {
    path: ['windowEnd'],
    message: 'O fim da janela não pode ser anterior ao início.',
  });

const mandateTarget = z.object({
  policyId: z.guid(),
  axisId: z.guid(),
  type: z.enum(MANDATE_TYPES),
  terms: mandateTerms,
});

// ---------- boletas ----------

const orderTerms = z.object({
  type: z.enum(ORDER_TYPES),
  direction: z.enum(TRADE_DIRECTIONS),
  tenor: text(16),
  lots: optionalPositive,
  notionalUsd: optionalPositive,
  price: z.number().positive(),
  priceUnit: optionalText(16),
  optionKind: nullableEnum(OPTION_KINDS),
  premium: optionalNumber,
  tradeDate: date,
  notes: optionalText(1000),
  commodity: nullableEnum(COMMODITIES),
  coveredSale: z.boolean().default(false),
  justification: optionalText(500),
});

// ---------- setup ----------

const commodityData = {
  capacity: z.number().positive(),
  unit: z.enum(MEASUREMENT_UNITS),
  priceReference: optionalText(64),
  currency: z.string().trim().length(3).toUpperCase(),
  sells: z.boolean().default(true),
};

const counterparty = z.object({
  name: text(200),
  type: z.enum(COUNTERPARTY_TYPES),
  document: optionalText(32),
  address: optionalText(300),
  country: optionalText(64),
  notionalLimitUsd: optionalPositive,
  mtmLimitUsd: optionalPositive,
});

export const schemas = {
  register: z.object({
    email: z.email(),
    password: z.string().min(8).max(128),
    fullName: text(150),
  }),
  login: z.object({ email: z.email(), password: z.string().min(1) }),

  // organização e setup
  organization: z.object({ name: text(150) }),
  companyProfile: z.object({
    corporateName: text(200),
    taxId: optionalText(32),
    headquarters: optionalText(200),
    group: optionalText(200),
    sector: z.enum(SECTORS),
    cropYearStartMonth: z.number().int().min(1).max(12),
    activeCrop: crop.nullish().transform((value) => value ?? null),
  }),
  industrialProfile: z
    .object({
      millingCapacity: optionalPositive,
      mixMinPct: optionalPct,
      mixMaxPct: optionalPct,
      mixGuidancePct: optionalPct,
    })
    .refine((i) => i.mixMinPct === null || i.mixMaxPct === null || i.mixMaxPct >= i.mixMinPct, {
      path: ['mixMaxPct'],
      message: 'O mix máximo não pode ser menor que o mínimo.',
    }),
  budget: z.object({
    cashCost: optionalPositive,
    economicFloor: optionalPositive,
    equivalentPrice: optionalPositive,
    targetMarginPct: optionalNumber,
  }),
  financials: z.object({
    cash: optionalNumber,
    creditLines: optionalNumber,
    monthlyFixedCost: optionalNumber,
    netDebt: optionalNumber,
    ebitda: optionalNumber,
    usdDebt: optionalNumber,
    referenceDate: optionalDate,
  }),
  addCommodity: z.object({ commodity: z.enum(COMMODITIES), ...commodityData }),
  updateCommodity: z.object(commodityData),

  // membros e grupos
  addMember: z.object({
    email: z.email(),
    ruleCode: z.string().trim().max(64).nullish().transform((v) => v || null),
    desk: nullableEnum(DESKS),
  }),
  ruleCodes: z.object({ ruleCodes: z.array(text(64)).max(30).default([]) }),
  transferOwnership: z.object({ memberId: z.guid() }),
  rule: z.object({ name: text(128), roleCodes: z.array(text(64)).min(1).max(40) }),
  memberDesk: z.object({ desk: nullableEnum(DESKS) }),
  createGroup: z.object({
    name: text(150),
    ruleCodes: z.array(text(64)).default([]),
    parentGroupId: z.guid().nullish().transform((v) => v ?? null),
  }),
  moveGroup: z.object({ parentGroupId: z.guid() }),
  addGroupMember: z.object({ memberId: z.guid() }),
  renameGroup: z.object({ name: text(150) }),
  groupMemberParams: z.object({ id: z.guid(), memberId: z.guid() }),

  // contrapartes
  counterparty,
  counterpartiesQuery: z.object({ onlyHomologated: z.stringbool().default(false) }),
  homologation: z.object({ homologated: z.boolean() }),

  // políticas
  createPolicy: z
    .object({ ...policyHeader, version: text(16), useTemplate: z.boolean().default(true) })
    .refine(validRange, rangeMessage),
  updatePolicy: z.object(policyHeader).refine(validRange, rangeMessage),
  policyLimits,
  approvePolicy: z.object({ approvalRecord: text(200) }),
  openPolicyVersion: z.object({ version: text(16), reason: optionalText(1000) }),
  policyAxis,
  coverageBand,
  policyInstrument,
  policiesQuery: pageQuery.transform(toPage),

  // mandatos
  issueMandate: mandateTarget,
  updateMandate: z.object({ terms: mandateTerms }),
  mandatesQuery: pageQuery
    .extend({ policyId: z.guid().optional(), status: z.enum(MANDATE_STATUSES).optional() })
    .transform(({ policyId, status, ...page }) => ({
      policyId: policyId ?? null,
      status: status ?? null,
      page: { page: page.page, pageSize: page.pageSize },
    })),
  note,
  requiredNote,

  // boletas
  registerOrder: z.object({ mandateId: z.guid().nullable().default(null), counterpartyId: z.guid(), terms: orderTerms }),
  linkOrderMandate: z.object({ mandateId: z.guid(), justification: z.string().trim().min(1, 'Justifique o vínculo a posteriori.').max(500) }),
  updateOrder: z.object({ counterpartyId: z.guid(), terms: orderTerms }),
  confirmOrder: z.object({ receivedOn: date }),
  ordersQuery: pageQuery
    .extend({
      mandateId: z.guid().optional(),
      approval: z.enum(APPROVAL_STATUSES).optional(),
      confirmation: z.enum(CONFIRMATION_STATUSES).optional(),
      withoutMandate: z.enum(['true', 'false']).optional(),
      onlyOutside: z.enum(['true', 'false']).optional(),
    })
    .transform(({ mandateId, approval, confirmation, withoutMandate, onlyOutside, ...page }) => ({
      mandateId: mandateId ?? null,
      approval: approval ?? null,
      confirmation: confirmation ?? null,
      withoutMandate: withoutMandate === 'true',
      onlyOutside: onlyOutside === 'true',
      page: { page: page.page, pageSize: page.pageSize },
    })),

  timelineQuery: timelineFilters.extend({
    limit: z.coerce.number().int().min(1).max(500).optional(),
  }),

  /** Auditoria paginada: até 200 por página (a exportação percorre as páginas). */
  timelinePageQuery: timelineFilters
    .extend({
      page: z.coerce.number().int().min(1).default(1),
      pageSize: z.coerce.number().int().min(1).max(200).default(25),
    })
    .transform(toPage),

  // uploads
  fileKindParam: z.object({ kind: z.enum(FILE_KINDS) }),
  uploadFile: z.object({
    kind: z.enum(FILE_KINDS),
    fileName: z
      .string({ error: 'Informe o nome do arquivo (header X-File-Name).' })
      .trim()
      .min(1, 'Informe o nome do arquivo (header X-File-Name).')
      .max(255),
    contentType: z.string().trim().max(200).default('application/octet-stream'),
  }),
  filesQuery: pageQuery
    .extend({ kind: z.enum(FILE_KINDS).optional() })
    .transform(({ kind, ...page }) => ({ kind: kind ?? null, page: { page: page.page, pageSize: page.pageSize } })),
  fileLinesQuery: pageQuery
    .extend({
      pageSize: z.coerce.number().int().min(1).max(200).default(50),
      status: z.enum(FILE_LINE_STATUSES).optional(),
    })
    .transform(({ status, ...page }) => ({ status: status ?? null, page: { page: page.page, pageSize: page.pageSize } })),
  fileEventsQuery: z.object({ fileId: z.guid().optional() }).transform(({ fileId }) => ({ fileId: fileId ?? null })),
};
