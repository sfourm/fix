/** Conjuntos de dados do core que podem ser pesquisados, filtrados e agregados em dashboards. */
export const DATA_SOURCES = ['orders', 'mandates', 'counterparties', 'policies'] as const;

export type DataSource = (typeof DATA_SOURCES)[number];
