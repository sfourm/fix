import type { DataSource } from '../../../domain/common/data-source.js';
import type { CounterpartyResponse } from '../../counterparties/responses/counterparty.response.js';
import type { MandateResponse } from '../../mandates/responses/mandate.response.js';
import type { OrderResponse } from '../../orders/responses/order.response.js';
import type { PolicySummaryResponse } from '../../policies/responses/policy-summary.response.js';
import type { DataSourceDefinition, FieldDefinition, FieldType, ValueFormat } from './field-definition.js';
import { optionLabels } from './option-labels.js';

type Field<T> = FieldDefinition<T>;

/** Atalhos: dimensões (agrupáveis) e medidas (numéricas). */
const dim = <T>(key: string, label: string, type: FieldType, get: Field<T>['get'], extra: Partial<Field<T>> = {}): Field<T> => ({
  key,
  label,
  type,
  format: type === 'date' ? 'date' : 'text',
  groupable: type !== 'id',
  measurable: false,
  get,
  ...extra,
});

const measure = <T>(key: string, label: string, format: ValueFormat, get: Field<T>['get']): Field<T> => ({
  key,
  label,
  type: 'number',
  format,
  groupable: false,
  measurable: true,
  get,
});

/** "2026-09-25" -> "2026-09" (agrupamento mensal, ordena cronologicamente). */
const month = (date: string | null) => (date ? date.slice(0, 7) : null);

const orders: DataSourceDefinition<OrderResponse> = {
  key: 'orders',
  label: 'Boletas de hedge',
  role: 'view_order',
  fields: [
    dim('approval', 'Aprovação', 'enum', (o) => o.approval, { options: optionLabels.approval }),
    dim('confirmation', 'Confirmation', 'enum', (o) => o.confirmation, { options: optionLabels.confirmation }),
    dim('type', 'Instrumento', 'enum', (o) => o.terms.type, { options: optionLabels.orderType }),
    dim('direction', 'Operação', 'enum', (o) => o.terms.direction, { options: optionLabels.direction }),
    dim('optionKind', 'Tipo da opção', 'enum', (o) => o.terms.optionKind, { options: optionLabels.optionKind }),
    dim('commodity', 'Commodity', 'enum', (o) => o.commodity, { options: optionLabels.commodity }),
    dim('counterpartyName', 'Contraparte', 'text', (o) => o.counterpartyName),
    dim('mandateTitle', 'Mandato', 'text', (o) => o.mandateTitle),
    dim('tenor', 'Tela / vencimento', 'text', (o) => o.terms.tenor),
    dim('tradeDate', 'Data do trade', 'date', (o) => o.terms.tradeDate, { groupable: false }),
    dim('tradeMonth', 'Mês do trade', 'text', (o) => month(o.terms.tradeDate), { format: 'month' }),
    dim('confirmationOverdue', 'Confirmation atrasado', 'boolean', (o) => o.confirmationOverdue),
    dim('mandateId', 'Mandato (id)', 'id', (o) => o.mandateId),
    dim('counterpartyId', 'Contraparte (id)', 'id', (o) => o.counterpartyId),
    measure('lots', 'Lotes', 'decimal', (o) => o.terms.lots),
    measure('notionalUsd', 'Nocional (US$)', 'usd', (o) => o.terms.notionalUsd),
    measure('price', 'Preço / strike / taxa', 'decimal', (o) => o.terms.price),
    measure('premium', 'Prêmio', 'decimal', (o) => o.terms.premium),
  ],
};

const mandates: DataSourceDefinition<MandateResponse> = {
  key: 'mandates',
  label: 'Mandatos',
  role: 'view_mandate',
  fields: [
    dim('status', 'Status', 'enum', (m) => m.status, { options: optionLabels.mandateStatus }),
    dim('type', 'Tipo', 'enum', (m) => m.type, { options: optionLabels.mandateType }),
    dim('compliance', 'Enquadramento', 'enum', (m) => m.compliance.status, { options: optionLabels.compliance }),
    dim('commodity', 'Commodity', 'enum', (m) => m.terms.commodity, { options: optionLabels.commodity }),
    dim('title', 'Título', 'text', (m) => m.terms.title),
    dim('policyCode', 'Política', 'text', (m) => m.policyCode.toUpperCase()),
    dim('axisCode', 'Eixo', 'text', (m) => m.axisCode),
    dim('tenor', 'Tela / vencimento', 'text', (m) => m.terms.tenor),
    dim('windowEnd', 'Fim da janela', 'date', (m) => m.terms.windowEnd, { groupable: false }),
    dim('policyId', 'Política (id)', 'id', (m) => m.policyId),
    measure('quantity', 'Quantidade autorizada', 'decimal', (m) => m.terms.quantity),
    measure('consumed', 'Consumido', 'decimal', (m) => m.consumed),
    measure('balance', 'Saldo', 'decimal', (m) => m.balance),
  ],
};

const counterparties: DataSourceDefinition<CounterpartyResponse> = {
  key: 'counterparties',
  label: 'Contrapartes',
  role: 'view_counterparties',
  fields: [
    dim('type', 'Tipo', 'enum', (c) => c.type, { options: optionLabels.counterpartyType }),
    dim('isHomologated', 'Homologada', 'boolean', (c) => c.isHomologated),
    dim('name', 'Nome', 'text', (c) => c.name),
    dim('country', 'País', 'text', (c) => c.country),
    measure('notionalLimitUsd', 'Limite nocional (US$)', 'usd', (c) => c.notionalLimitUsd),
    measure('mtmLimitUsd', 'Limite MtM (US$)', 'usd', (c) => c.mtmLimitUsd),
  ],
};

const policies: DataSourceDefinition<PolicySummaryResponse> = {
  key: 'policies',
  label: 'Políticas',
  role: 'view_policy',
  fields: [
    dim('status', 'Status', 'enum', (p) => p.status, { options: optionLabels.policyStatus }),
    dim('code', 'Código', 'text', (p) => p.code.toUpperCase()),
    dim('version', 'Versão', 'text', (p) => p.version),
    dim('validFrom', 'Início da vigência', 'date', (p) => p.validFrom, { groupable: false }),
    dim('validTo', 'Fim da vigência', 'date', (p) => p.validTo, { groupable: false }),
    measure('axesCount', 'Eixos', 'integer', (p) => p.axesCount),
  ],
};

export const dataSourceCatalog: Record<DataSource, DataSourceDefinition<any>> = { orders, mandates, counterparties, policies };

export function findField(source: DataSource, key: string): FieldDefinition<unknown> | undefined {
  return dataSourceCatalog[source].fields.find((f) => f.key === key);
}
