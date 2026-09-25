const dateFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeZone: 'UTC' });
const dateTimeFormatter = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' });

/** "2026-01-31" -> "31/01/2026" */
export function formatDate(value: string | null | undefined): string {
  return value ? dateFormatter.format(new Date(`${value}T00:00:00Z`)) : '—';
}

export function formatDateTime(value: string | null | undefined): string {
  return value ? dateTimeFormatter.format(new Date(value)) : '—';
}

/** Número pt-BR; null vira "—". */
export function formatNumber(value: number | null | undefined, digits = 2): string {
  return value === null || value === undefined
    ? '—'
    : value.toLocaleString('pt-BR', { minimumFractionDigits: 0, maximumFractionDigits: digits });
}

export function formatPct(value: number | null | undefined): string {
  return value === null || value === undefined ? '—' : `${formatNumber(value, 2)}%`;
}

export function formatUsd(value: number | null | undefined): string {
  return value === null || value === undefined
    ? '—'
    : value.toLocaleString('pt-BR', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 });
}

/** Data de hoje (yyyy-MM-dd) no fuso local, para campos de data. */
export function today(): string {
  const now = new Date();
  return new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

/** Texto opcional de formulário: vazio vira null. */
export const orNull = (value: string | null | undefined): string | null => (value?.trim() ? value.trim() : null);
