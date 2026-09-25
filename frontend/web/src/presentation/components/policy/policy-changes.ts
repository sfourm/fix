import { instrumentPermissionLabel, policyStatusLabel, riskFactorLabel } from '@/domain/labels';
import type { InstrumentPermission, PolicyStatus, PolicyVersion, RiskFactor } from '@/domain/policy';
import type { TimelineEntry } from '@/domain/timeline';

/**
 * Alterações da política por versão, a partir da auditoria do agregado (timeline "Policy").
 * Cada registro pertence à versão vigente no momento: a abertura de versão grava Version: v1.0 → v1.1 (e um
 * PolicyVersion com a versão nova), então tudo o que vem depois conta para a versão nova.
 */

export type ChangeSection = 'header' | 'limits' | 'axes' | 'bands' | 'instruments';

export interface ChangeLine {
  label: string;
  old?: string;
  new?: string;
}

export interface Change {
  id: string;
  section: ChangeSection;
  kind: 'added' | 'removed' | 'updated';
  /** Assunto: "Fixação de preço (POL-PRE)", "Safra corrente · 26/27", "Horizonte máximo de hedge"… */
  subject: string;
  lines: ChangeLine[];
  authorId: string | null;
  occurredAt: string;
}

export interface VersionChanges {
  version: string;
  changes: Change[];
  /** Primeira versão: o conteúdo nasce inteiro (modelo ou cadastro inicial). */
  initial: boolean;
}

export const sectionLabel: Record<ChangeSection, string> = {
  header: 'Dados da política',
  limits: 'Parâmetros',
  axes: 'Eixos',
  bands: 'Bandas de cobertura',
  instruments: 'Instrumentos',
};

export const limitLabel: Record<string, string> = {
  HedgeHorizonYears: 'Horizonte máximo de hedge (anos)',
  AbsoluteCeilingPct: 'Teto absoluto de cobertura (%)',
  CoveredCallMaxPct: 'Venda coberta de opções (%)',
  FxFixedMinPct: 'NDF mínimo sobre a receita fixada (%)',
  FxFixedMaxPct: 'NDF máximo sobre a receita fixada (%)',
  FxUnfixedMaxPct: 'Proteção antecipada da receita não fixada (%)',
  MarginCashMaxPct: 'Margem em corretoras sobre o caixa (%)',
  PhysicalConcentrationMaxPct: 'Concentração por contraparte · físico (%)',
  FinancialConcentrationMaxPct: 'Concentração por contraparte · financeiro (%)',
  LogisticsDeadlineMonths: 'Prazo-limite para contratar o frete (meses)',
  FreightCeilingPct: 'Teto de frete (%)',
};

const fieldLabel: Record<string, string> = {
  Code: 'Código',
  Title: 'Título',
  Description: 'Descrição',
  'Validity.StartsOn': 'Vigência de',
  'Validity.EndsOn': 'Vigência até',
  Factor: 'Fator de risco',
  Statement: 'Diretriz',
  LimitDescription: 'Limite',
  Approver: 'Aprovador',
  Restrictions: 'Restrições',
  Horizon: 'Horizonte',
  Crop: 'Safra',
  MinPct: 'Mínimo (%)',
  MaxPct: 'Máximo (%)',
  Note: 'Observação',
  Name: 'Nome',
  Permission: 'Permissão',
  Condition: 'Condição',
};

/** Campos do ciclo de vida: aparecem nas etapas da versão, não como alteração de conteúdo. */
const LIFECYCLE = new Set(['Status', 'Version', 'ApprovedOn', 'ApprovalRecord']);

const CHILD: Record<string, ChangeSection> = { PolicyAxis: 'axes', CoverageBand: 'bands', PolicyInstrument: 'instruments' };

function show(field: string, value: unknown): string {
  if (value === null || value === undefined || value === '') return '—';
  if (typeof value === 'boolean') return value ? 'sim' : 'não';
  if (Array.isArray(value)) return value.length ? value.join('; ') : '—';
  if (field === 'Factor') return riskFactorLabel[value as RiskFactor] ?? String(value);
  if (field === 'Permission') return instrumentPermissionLabel[value as InstrumentPermission] ?? String(value);
  if (field === 'Status') return policyStatusLabel[value as PolicyStatus] ?? String(value);
  if (typeof value === 'number') return value.toLocaleString('pt-BR');
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) return value.split('-').reverse().join('/');
  return typeof value === 'object' ? JSON.stringify(value) : String(value);
}

const isDiff = (v: unknown): v is { old: unknown; new: unknown } => !!v && typeof v === 'object' && 'new' in v && 'old' in v;
const technical = (field: string) => field === 'Id' || field.endsWith('Id');

/** Versão explícita num registro (abertura de versão ou criação da política). */
function versionOf(entry: TimelineEntry): string | null {
  const c = entry.changes;
  if (typeof c['PolicyVersion.Version'] === 'string') return c['PolicyVersion.Version'] as string;
  if (isDiff(c.Version)) return String(c.Version.new);
  if (entry.action === 'Created' && typeof c.Version === 'string') return c.Version;
  return null;
}

function subjectOf(section: ChangeSection, values: Record<string, unknown>, fallback: string): string {
  if (section === 'axes') return [values.Title, values.Code && `(${values.Code})`].filter(Boolean).join(' ') || fallback;
  if (section === 'bands') return [values.Horizon, values.Crop].filter(Boolean).join(' · ') || fallback;
  if (section === 'instruments') return (values.Name as string) || fallback;
  if (section === 'header') return (values.Title as string) || fallback;
  return fallback;
}

/** Converte um registro em alterações legíveis (um registro pode tocar dados gerais e parâmetros ao mesmo tempo). */
function toChanges(entry: TimelineEntry): Change[] {
  const base = { authorId: entry.authorId, occurredAt: entry.occurredAt };
  const byPrefix = new Map<string, [string, unknown][]>();
  for (const [field, value] of Object.entries(entry.changes)) {
    const dot = field.indexOf('.');
    const prefix = dot > 0 && CHILD[field.slice(0, dot)] ? field.slice(0, dot) : dot > 0 && field.startsWith('Limits.') ? 'Limits' : dot > 0 && field.startsWith('PolicyVersion.') ? 'PolicyVersion' : '';
    const rest = prefix ? field.slice(prefix.length + 1) : field;
    if (prefix === 'PolicyVersion' || technical(rest) || (!prefix && LIFECYCLE.has(rest))) continue;
    byPrefix.set(prefix, [...(byPrefix.get(prefix) ?? []), [rest, value]]);
  }

  const out: Change[] = [];
  byPrefix.forEach((fields, prefix) => {
    if (prefix === 'Limits') {
      fields.forEach(([f, v]) =>
        out.push({
          id: `${entry.id}-${f}`,
          section: 'limits',
          kind: entry.action === 'Created' ? 'added' : 'updated',
          subject: limitLabel[f] ?? f,
          lines: [isDiff(v) ? { label: '', old: show(f, v.old), new: show(f, v.new) } : { label: '', new: show(f, v) }],
          ...base,
        }),
      );
      return;
    }

    const section: ChangeSection = prefix ? CHILD[prefix]! : 'header';
    const current = Object.fromEntries(fields.map(([f, v]) => [f, isDiff(v) ? v.new : v]));
    const values = current;
    const kind = entry.action === 'Created' ? 'added' : entry.action === 'Deleted' ? 'removed' : 'updated';
    const lines: ChangeLine[] =
      kind === 'updated'
        ? fields.map(([f, v]) => (isDiff(v) ? { label: fieldLabel[f] ?? f, old: show(f, v.old), new: show(f, v.new) } : { label: fieldLabel[f] ?? f, new: show(f, v) }))
        : fields
            .filter(([f]) => section === 'header' || !['Code', 'Title', 'Name', 'Horizon', 'Crop'].includes(f))
            .map(([f, v]) => ({ label: fieldLabel[f] ?? f, new: show(f, isDiff(v) ? v.new : v) }))
            .filter((l) => l.new !== '—');
    out.push({ id: entry.id + prefix, section, kind, subject: subjectOf(section, values, section === 'header' ? 'Política' : sectionLabel[section]), lines, ...base });
  });
  return out;
}

/** Agrupa a auditoria da política por versão (mais recente primeiro dentro de cada versão). */
export function changesByVersion(entries: TimelineEntry[]): Map<string, VersionChanges> {
  const ordered = [...entries].sort((a, b) => a.occurredAt.localeCompare(b.occurredAt));
  const first = ordered.map(versionOf).find(Boolean) ?? 'v1.0';
  const result = new Map<string, VersionChanges>();
  let current = first;
  for (const entry of ordered) {
    current = versionOf(entry) ?? current;
    let bucket = result.get(current);
    if (!bucket) {
      bucket = { version: current, changes: [], initial: current === first };
      result.set(current, bucket);
    }
    bucket.changes.push(...toChanges(entry));
  }
  result.forEach((b) => b.changes.reverse());
  return result;
}

/** Etapa da versão com o instante exato (a versão só guarda a data; o horário vem da auditoria). */
export type TimedVersion = PolicyVersion & { at: string | null };

/**
 * Liga cada etapa de policy.versions ao registro de auditoria que a criou (PolicyVersion incluído), na ordem:
 * mesma versão e mesmo status, o primeiro ainda não usado. Sem auditoria, fica só a data.
 */
export function timedVersions(versions: PolicyVersion[], entries: TimelineEntry[] | null | undefined): TimedVersion[] {
  const created = (entries ?? [])
    .filter((e) => e.action === 'Created' && typeof e.changes['PolicyVersion.Version'] === 'string')
    .sort((a, b) => a.occurredAt.localeCompare(b.occurredAt))
    .map((e) => ({ version: e.changes['PolicyVersion.Version'] as string, status: e.changes['PolicyVersion.Status'] as string, at: e.occurredAt, used: false }));
  return versions.map((v) => {
    const hit = created.find((c) => !c.used && c.version === v.version && c.status === v.status);
    if (hit) hit.used = true;
    return { ...v, at: hit?.at ?? null };
  });
}
