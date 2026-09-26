import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { ApprovalStatus } from '../../../cross-cutting/enums/approval-status.js';
import type { ConfirmationStatus } from '../../../cross-cutting/enums/confirmation-status.js';
import type { MandateStatus } from '../../../cross-cutting/enums/mandate-status.js';
import type { Page } from '../../../cross-cutting/paging/page.js';
import type { DataSource } from '../../../domain/common/data-source.js';
import type { FilterCriterion } from '../../../domain/filters/filter-criterion.js';
import type { VisualizationCache } from '../../common/visualization-cache.js';
import type { CounterpartyService } from '../../counterparties/services/counterparty.service.js';
import type { MandateService } from '../../mandates/services/mandate.service.js';
import type { OrderService } from '../../orders/services/order.service.js';
import type { PolicyService } from '../../policies/services/policy.service.js';

/** Teto de linhas lidas do core por conjunto (proteção de memória e de tempo de resposta). */
export const MAX_ROWS = 5000;
const PAGE_SIZE = 100;

export interface SourceRows {
  rows: unknown[];
  /** O conjunto tinha mais linhas que MAX_ROWS. */
  truncated: boolean;
  cached: boolean;
}

export interface SourceRowReaderDeps {
  orders: OrderService;
  mandates: MandateService;
  counterparties: CounterpartyService;
  policies: PolicyService;
  cache: VisualizationCache;
}

/**
 * Lê as linhas de um conjunto de dados no core, sempre com o contexto do usuário (o core aplica tenant e roles).
 * Condições de igualdade que o core já sabe filtrar vão como parâmetro da listagem (menos dados trafegados);
 * as demais são aplicadas depois pelo motor de critérios. O resultado fica no cache de visualização.
 */
export class SourceRowReader {
  constructor(private readonly deps: SourceRowReaderDeps) {}

  async read(context: RequestContext, source: DataSource, criteria: readonly FilterCriterion[]): Promise<SourceRows> {
    const pushdown = SourceRowReader.pushdown(source, criteria);
    const key = `rows:${context.userId}:${source}:${JSON.stringify(pushdown)}`;

    const { value, cached } = await this.deps.cache.remember(context.organizationId!, key, () => this.fetch(context, source, pushdown));
    return { ...value, cached };
  }

  private async fetch(context: RequestContext, source: DataSource, pushdown: Record<string, string>): Promise<Omit<SourceRows, 'cached'>> {
    switch (source) {
      case 'orders':
        return this.paged((page) =>
          this.deps.orders.list({
            context,
            mandateId: pushdown.mandateId ?? null,
            approval: (pushdown.approval as ApprovalStatus | undefined) ?? null,
            confirmation: (pushdown.confirmation as ConfirmationStatus | undefined) ?? null,
            withoutMandate: false,
            onlyOutside: false,
            page,
          }),
        );
      case 'mandates':
        return this.paged((page) =>
          this.deps.mandates.list({
            context,
            policyId: pushdown.policyId ?? null,
            status: (pushdown.status as MandateStatus | undefined) ?? null,
            page,
          }),
        );
      case 'policies':
        return this.paged((page) => this.deps.policies.list({ context, page }));
      case 'counterparties': {
        const rows = await this.deps.counterparties.list({ context, onlyHomologated: pushdown.isHomologated === 'true' });
        return { rows, truncated: false };
      }
    }
  }

  private async paged(load: (page: { page: number; pageSize: number }) => Promise<Page<unknown>>): Promise<Omit<SourceRows, 'cached'>> {
    const rows: unknown[] = [];
    for (let page = 1; ; page++) {
      const result = await load({ page, pageSize: PAGE_SIZE });
      rows.push(...result.items);

      if (rows.length >= MAX_ROWS) {
        return { rows: rows.slice(0, MAX_ROWS), truncated: result.totalCount > MAX_ROWS };
      }

      if (result.items.length < PAGE_SIZE || rows.length >= result.totalCount) {
        return { rows, truncated: false };
      }
    }
  }

  /** Igualdades suportadas pelas listagens do core, por conjunto (campo do catálogo → parâmetro da listagem). */
  private static pushdown(source: DataSource, criteria: readonly FilterCriterion[]): Record<string, string> {
    const supported: Record<DataSource, string[]> = {
      orders: ['mandateId', 'approval', 'confirmation'],
      mandates: ['policyId', 'status'],
      counterparties: ['isHomologated'],
      policies: [],
    };

    const params: Record<string, string> = {};
    for (const c of criteria) {
      const onlyHomologated = c.field === 'isHomologated' && c.value !== true;
      if (c.operator === 'eq' && supported[source].includes(c.field) && !onlyHomologated && !(c.field in params)) {
        params[c.field] = String(c.value);
      }
    }

    return params;
  }
}
