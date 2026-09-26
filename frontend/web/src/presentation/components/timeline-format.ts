import { childEntityLabel } from '@/domain/labels';
import type { TimelineEntry } from '@/domain/timeline';

/** Texto das alterações de um registro de auditoria: usado na lista e na exportação. */

const fieldLabel: Record<string, string> = {
  Title: 'Título',
  Name: 'Nome',
  Slug: 'Identificador',
  Code: 'Código',
  Version: 'Versão',
  Description: 'Descrição',
  Status: 'Status',
  Approval: 'Aprovação',
  Confirmation: 'Confirmação',
  Desk: 'Mesa',
  IsDefault: 'Padrão',
  IsHomologated: 'Homologada',
  ApprovalRecord: 'Ata',
  DecisionNote: 'Justificativa',
  Consumed: 'Consumido',
};

/** 'PolicyAxis.Limits.Title' -> 'Eixo · Limits · Título' (prefixo = entidade filha do agregado). */
function labelOf(field: string): string {
  return field
    .split('.')
    .map((part, index) => (index === 0 && childEntityLabel[part]) || fieldLabel[part] || part)
    .join(' · ');
}

function show(value: unknown): string {
  if (value === null || value === undefined || value === '') return '∅';
  if (typeof value === 'boolean') return value ? 'sim' : 'não';
  return typeof value === 'object' ? JSON.stringify(value) : String(value);
}

/** Campos de negócio alterados ("Campo: antes → depois"); ids técnicos (chaves estrangeiras) ficam de fora. */
export function describeChanges(entry: TimelineEntry): string[] {
  return Object.entries(entry.changes)
    .filter(([field]) => !field.endsWith('Id') && !field.endsWith('.Id'))
    .map(([field, value]) => {
      const label = labelOf(field);
      if (value && typeof value === 'object' && 'new' in value) {
        const change = value as { old: unknown; new: unknown };
        return `${label}: ${show(change.old)} → ${show(change.new)}`;
      }

      return `${label}: ${show(value)}`;
    });
}
