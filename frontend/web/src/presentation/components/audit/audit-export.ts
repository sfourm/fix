import { entityTypeLabel, timelineActionLabel } from '@/domain/labels';
import type { Page } from '@/domain/page';
import type { TimelineEntry } from '@/domain/timeline';
import { describeChanges } from '../timeline-format';

/**
 * Exportação da auditoria com os filtros atuais (todas as páginas, até MAX_ROWS registros):
 * planilha CSV (abre no Excel: separador ";" e BOM UTF-8) ou PDF em paisagem.
 */
export const MAX_ROWS = 5000;
const PAGE_SIZE = 200;

export interface AuditExportInput {
  fetchPage: (page: number, pageSize: number) => Promise<Page<TimelineEntry>>;
  authorOf: (entry: TimelineEntry) => string;
  /** Resumo dos filtros aplicados (vai no cabeçalho do PDF). */
  filtersLabel: string;
  organizationName: string;
  /** Escopo (ex.: "POL-2026"): vai no título do PDF e no nome do arquivo. */
  scopeLabel?: string | undefined;
}

export interface AuditExportResult {
  exported: number;
  total: number;
}

async function fetchAll(input: AuditExportInput): Promise<{ entries: TimelineEntry[]; total: number }> {
  const entries: TimelineEntry[] = [];
  let page = 1;
  let total = 0;
  do {
    const result = await input.fetchPage(page, PAGE_SIZE);
    total = result.totalCount;
    entries.push(...result.items);
    page++;
    if (!result.items.length) break;
  } while (entries.length < Math.min(total, MAX_ROWS));
  return { entries: entries.slice(0, MAX_ROWS), total };
}

const when = (iso: string) => new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'medium' });
/** Carimbo do nome do arquivo no horário local: 202609252236. */
/** "auditoria-pol-2026-202609252236" */
const fileName = (input: AuditExportInput, ext: string) =>
  ['auditoria', input.scopeLabel?.toLowerCase().replace(/[^a-z0-9]+/g, '-'), stamp()].filter(Boolean).join('-') + '.' + ext;

const stamp = () => {
  const d = new Date();
  const two = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}${two(d.getMonth() + 1)}${two(d.getDate())}${two(d.getHours())}${two(d.getMinutes())}`;
};

function rows(entries: TimelineEntry[], input: AuditExportInput) {
  return entries.map((e) => ({
    when: when(e.occurredAt),
    entity: entityTypeLabel[e.entityType] ?? e.entityType,
    action: timelineActionLabel[e.action],
    author: input.authorOf(e),
    id: e.entityId,
    changes: describeChanges(e),
  }));
}

function download(blob: Blob, name: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = name;
  a.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}

export async function exportAuditCsv(input: AuditExportInput): Promise<AuditExportResult> {
  const { entries, total } = await fetchAll(input);
  const cell = (v: string) => `"${v.replace(/"/g, '""')}"`;
  const header = ['Data e hora', 'Entidade', 'Ação', 'Autor', 'Id da entidade', 'Alterações'];
  const lines = rows(entries, input).map((r) => [r.when, r.entity, r.action, r.author, r.id, r.changes.join(' | ')].map(cell).join(';'));
  const csv = '﻿' + [header.map(cell).join(';'), ...lines].join('\r\n');
  download(new Blob([csv], { type: 'text/csv;charset=utf-8' }), fileName(input, 'csv'));
  return { exported: entries.length, total };
}

export async function exportAuditPdf(input: AuditExportInput): Promise<AuditExportResult> {
  const [{ entries, total }, { jsPDF }, { default: autoTable }, { clean }] = await Promise.all([
    fetchAll(input),
    import('jspdf'),
    import('jspdf-autotable'),
    import('../policy/policy-pdf'),
  ]);
  const doc = new jsPDF({ unit: 'pt', format: 'a4', orientation: 'landscape' });
  const W = doc.internal.pageSize.getWidth();
  const M = 40;

  doc.setFillColor(168, 114, 12);
  doc.rect(0, 0, W, 5, 'F');
  doc.setFont('helvetica', 'bold');
  doc.setFontSize(9);
  doc.setTextColor(168, 114, 12);
  doc.text(clean(`FIX · ${input.organizationName}`), M, 40);
  doc.setFontSize(18);
  doc.setTextColor(29, 29, 31);
  doc.text(clean(input.scopeLabel ? `Auditoria · ${input.scopeLabel}` : 'Auditoria'), M, 62);
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(9);
  doc.setTextColor(110, 110, 115);
  doc.text(clean(`${input.filtersLabel} · ${entries.length} de ${total} registro(s) · gerado em ${new Date().toLocaleString('pt-BR')}`), M, 78);

  autoTable(doc, {
    startY: 92,
    margin: { left: M, right: M, bottom: 40 },
    head: [['Data e hora', 'Entidade', 'Ação', 'Autor', 'Alterações']],
    body: rows(entries, input).map((r) => [r.when, r.entity, r.action, r.author, r.changes.join('\n') || '-'].map(clean)),
    theme: 'plain',
    styles: { font: 'helvetica', fontSize: 7.5, cellPadding: 4, textColor: [29, 29, 31], lineColor: [228, 228, 232], lineWidth: { bottom: 0.5 }, valign: 'top' },
    headStyles: { fontStyle: 'bold', textColor: [110, 110, 115], lineWidth: { bottom: 0.8 } },
    columnStyles: { 0: { cellWidth: 82 }, 1: { cellWidth: 70 }, 2: { cellWidth: 55 }, 3: { cellWidth: 95 } },
  });

  const pages = doc.getNumberOfPages();
  for (let i = 1; i <= pages; i++) {
    doc.setPage(i);
    doc.setFontSize(8);
    doc.setTextColor(110, 110, 115);
    doc.text(`${i} / ${pages}`, W - M, doc.internal.pageSize.getHeight() - 20, { align: 'right' });
  }
  doc.save(fileName(input, 'pdf'));
  return { exported: entries.length, total };
}
