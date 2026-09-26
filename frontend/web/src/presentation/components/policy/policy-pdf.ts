import { instrumentPermissionLabel, policyStatusLabel, riskFactorLabel } from '@/domain/labels';
import type { Policy } from '@/domain/policy';
import { limitLabel, sectionLabel, type Change, type TimedVersion } from './policy-changes';

/**
 * Exporta uma versão da política em PDF (gerado no navegador; jsPDF carregado só na hora de exportar).
 * Conteúdo: cabeçalho, ciclo da versão (etapas e ata), o conteúdo completo quando é a versão atual
 * (parâmetros, eixos, bandas e instrumentos) e as alterações registradas nesta versão.
 */
export interface VersionPdfInput {
  policy: Policy;
  version: string;
  steps: TimedVersion[];
  changes: Change[];
  initial: boolean;
  previous: string | null;
  authorOf: (change: Change) => string;
}

const GOLD: [number, number, number] = [168, 114, 12];
const INK: [number, number, number] = [29, 29, 31];
const MUTED: [number, number, number] = [110, 110, 115];
const LINE: [number, number, number] = [228, 228, 232];

/** As fontes padrão do PDF só cobrem Latin-1: troca os símbolos fora dele. */
export const clean = (s: string | null | undefined) =>
  (s ?? '—')
    .replace(/[→]/g, '->')
    .replace(/[—–−]/g, '-')
    .replace(/…/g, '...')
    .replace(/[“”]/g, '"')
    .replace(/[‘’]/g, "'")
    .replace(/[≥]/g, '>=')
    .replace(/[≤]/g, '<=')
    .replace(/[^\u0000-ÿ]/g, '');

const date = (iso: string | null | undefined) => (iso ? iso.slice(0, 10).split('-').reverse().join('/') : '—');
const dateTime = (iso: string) => new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
const pascal = (key: string) => key.charAt(0).toUpperCase() + key.slice(1);

export async function exportVersionPdf(input: VersionPdfInput): Promise<void> {
  const [{ jsPDF }, { default: autoTable }] = await Promise.all([import('jspdf'), import('jspdf-autotable')]);
  const { policy, version, steps, changes } = input;
  const isCurrent = policy.version === version;

  const doc = new jsPDF({ unit: 'pt', format: 'a4' });
  const W = doc.internal.pageSize.getWidth();
  const M = 48;
  let y = 56;

  const lastY = () => (doc as unknown as { lastAutoTable?: { finalY: number } }).lastAutoTable?.finalY ?? y;
  const ensure = (space: number) => {
    if (y + space > doc.internal.pageSize.getHeight() - 60) {
      doc.addPage();
      y = 56;
    }
  };
  const heading = (text: string, sub?: string) => {
    ensure(60);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.setTextColor(...INK);
    doc.text(clean(text), M, y);
    if (sub) {
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(9);
      doc.setTextColor(...MUTED);
      doc.text(clean(sub), W - M, y, { align: 'right' });
    }
    y += 10;
  };
  const table = (head: string[], body: string[][], widths?: Record<number, number>) => {
    autoTable(doc, {
      startY: y,
      margin: { left: M, right: M },
      head: [head.map(clean)],
      body: body.map((r) => r.map(clean)),
      theme: 'plain',
      styles: { font: 'helvetica', fontSize: 8.5, cellPadding: { top: 5, bottom: 5, left: 6, right: 6 }, textColor: INK, lineColor: LINE, lineWidth: { bottom: 0.5 }, valign: 'top' },
      headStyles: { fontStyle: 'bold', textColor: MUTED, fontSize: 8, lineWidth: { bottom: 0.8 } },
      columnStyles: Object.fromEntries(Object.entries(widths ?? {}).map(([k, w]) => [k, { cellWidth: w }])),
    });
    y = lastY() + 22;
  };

  // ---------- Cabeçalho ----------
  doc.setFillColor(...GOLD);
  doc.rect(0, 0, W, 6, 'F');
  doc.setFont('helvetica', 'bold');
  doc.setFontSize(9);
  doc.setTextColor(...GOLD);
  doc.text(clean(`FIX · Política de riscos · ${policy.code.toUpperCase()}`), M, y);
  y += 22;
  doc.setFontSize(20);
  doc.setTextColor(...INK);
  doc.text(doc.splitTextToSize(clean(policy.title), W - M * 2) as string[], M, y);
  y += 26;
  const last = steps[steps.length - 1];
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(11);
  doc.setTextColor(...MUTED);
  doc.text(
    clean(`Versão ${version}${last ? ` · ${policyStatusLabel[last.status]}` : ''} · vigência ${date(policy.validFrom)} a ${date(policy.validTo)}`),
    M,
    y,
  );
  y += 14;
  doc.setFontSize(8.5);
  doc.text(clean(`Gerado em ${new Date().toLocaleString('pt-BR')}`), M, y);
  y += 18;
  doc.setDrawColor(...LINE);
  doc.line(M, y, W - M, y);
  y += 26;

  // ---------- Ciclo ----------
  heading('Ciclo da versão');
  table(
    ['Etapa', 'Data e hora', 'Registro'],
    steps.map((s) => [policyStatusLabel[s.status], s.at ? dateTime(s.at) : date(s.date), s.note ?? '—']),
    { 0: 110, 1: 95 },
  );

  // ---------- Conteúdo (só a versão atual tem o retrato completo) ----------
  if (isCurrent) {
    if (policy.description) {
      heading('Descrição');
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(9.5);
      doc.setTextColor(...INK);
      const lines = doc.splitTextToSize(clean(policy.description), W - M * 2) as string[];
      ensure(lines.length * 12);
      doc.text(lines, M, y + 4);
      y += lines.length * 12 + 22;
    }
    heading('Parâmetros');
    table(
      ['Parâmetro', 'Valor'],
      Object.entries(policy.limits).map(([k, v]) => [limitLabel[pascal(k)] ?? k, Number(v).toLocaleString('pt-BR')]),
      { 1: 90 },
    );
    heading('Eixos', `${policy.axes.length}`);
    table(
      ['Código', 'Eixo', 'Fator', 'Aprovador', 'Diretriz e limite'],
      policy.axes.map((a) => [a.code, a.title, riskFactorLabel[a.factor], a.approver ?? '—', [a.statement, a.limitDescription].filter(Boolean).join('\n') || '—']),
      { 0: 58, 1: 100, 2: 50, 3: 85 },
    );
    heading('Bandas de cobertura', `${policy.bands.length}`);
    table(
      ['Horizonte', 'Safra', 'Mínimo', 'Máximo', 'Observação'],
      policy.bands.map((b) => [b.horizon, b.crop, `${b.minPct}%`, `${b.maxPct}%`, b.note ?? '—']),
      { 0: 95, 1: 50, 2: 50, 3: 50 },
    );
    heading('Instrumentos', `${policy.instruments.length}`);
    table(
      ['Instrumento', 'Permissão', 'Condição'],
      policy.instruments.map((i) => [i.name, instrumentPermissionLabel[i.permission], i.condition ?? '—']),
      { 0: 190, 1: 70 },
    );
  }

  // ---------- Alterações ----------
  heading(input.initial ? 'Conteúdo registrado na criação' : `Alterações${input.previous ? ` desde ${input.previous}` : ''}`, `${changes.length}`);
  if (!changes.length) {
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9.5);
    doc.setTextColor(...MUTED);
    doc.text(clean('Nenhuma alteração de conteúdo nesta versão.'), M, y + 6);
    y += 28;
  } else {
    const kind = { added: 'Incluído', updated: 'Alterado', removed: 'Removido' } as const;
    table(
      ['Seção', 'Item', 'Tipo', 'Detalhe', 'Por'],
      changes.map((c) => [
        sectionLabel[c.section],
        c.subject,
        kind[c.kind],
        c.lines.map((l) => `${l.label ? `${l.label}: ` : ''}${l.old !== undefined ? `${l.old} -> ` : ''}${l.new ?? ''}`).join('\n') || '—',
        `${input.authorOf(c)}\n${dateTime(c.occurredAt)}`,
      ]),
      { 0: 62, 1: 100, 2: 50, 4: 75 },
    );
  }

  if (!isCurrent) {
    ensure(30);
    doc.setFont('helvetica', 'italic');
    doc.setFontSize(8.5);
    doc.setTextColor(...MUTED);
    doc.text(
      doc.splitTextToSize(clean(`O retrato completo (parâmetros, eixos, bandas e instrumentos) acompanha a versão atual (${policy.version}); para versões anteriores o documento traz o ciclo e as alterações registradas.`), W - M * 2) as string[],
      M,
      y,
    );
  }

  // ---------- Rodapé ----------
  const pages = doc.getNumberOfPages();
  for (let i = 1; i <= pages; i++) {
    doc.setPage(i);
    const H = doc.internal.pageSize.getHeight();
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8);
    doc.setTextColor(...MUTED);
    doc.text(clean(`${policy.code.toUpperCase()} · versão ${version}`), M, H - 28);
    doc.text(`${i} / ${pages}`, W - M, H - 28, { align: 'right' });
  }

  doc.save(`${policy.code.toLowerCase()}-${version}.pdf`);
}
