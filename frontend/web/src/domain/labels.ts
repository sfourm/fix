import type { Commodity, MeasurementUnit } from './common';
import type { CounterpartyType } from './counterparty';
import type { ComplianceStatus, MandateStatus, MandateType } from './mandate';
import type { ApprovalStatus, ConfirmationStatus, OptionKind, OrderType, TradeDirection } from './order';
import type { Desk, Sector } from './organization';
import type { InstrumentPermission, PolicyStatus, RiskFactor } from './policy';
import type { TimelineEntry } from './timeline';

/** Tom visual de um status (classe badge-*). */
export type Tone = 'success' | 'warning' | 'danger' | 'info' | 'primary' | 'neutral';

export const commodityLabel: Record<Commodity, string> = {
  RawSugar: 'Açúcar VHP · NY11',
  WhiteSugar: 'Açúcar branco · Londres nº5',
  HydratedEthanol: 'Etanol hidratado',
  AnhydrousEthanol: 'Etanol anidro',
  Corn: 'Milho · CBOT',
  Soybean: 'Soja · CBOT',
};

export const unitLabel: Record<MeasurementUnit, string> = {
  Lots: 'lotes',
  Tonnes: 't',
  Bags: 'sacas',
  CubicMeters: 'm³',
  Pounds: 'lb',
  Usd: 'US$',
};

export const sectorLabel: Record<Sector, string> = {
  SugarEnergy: 'Sucroenergético',
  Grains: 'Grãos',
  Livestock: 'Pecuária',
};

export const deskLabel: Record<Desk, string> = {
  ExecutionDesk: 'Mesa de execução',
  Commercial: 'Comercial',
  Logistics: 'Logística',
  Board: 'Diretoria',
  RiskControl: 'Controle de riscos',
};

export const counterpartyTypeLabel: Record<CounterpartyType, string> = {
  Trading: 'Trading',
  BankTrading: 'Banco / trading',
  OtcCounterparty: 'Contraparte OTC',
  Broker: 'Corretora',
  Carrier: 'Transportadora',
  Producer: 'Produtor',
};

export const policyStatusLabel: Record<PolicyStatus, string> = {
  Draft: 'Rascunho',
  UnderApproval: 'Em aprovação',
  Active: 'Vigente',
  Superseded: 'Substituída',
};

export const policyStatusTone: Record<PolicyStatus, Tone> = {
  Draft: 'neutral',
  UnderApproval: 'warning',
  Active: 'success',
  Superseded: 'info',
};

export const riskFactorLabel: Record<RiskFactor, string> = {
  Physical: 'Físico',
  Price: 'Preço',
  Currency: 'Moeda',
  Freight: 'Frete',
};

export const instrumentPermissionLabel: Record<InstrumentPermission, string> = {
  Allowed: 'Permitido',
  Capped: 'Com teto',
  Forbidden: 'Vedado',
};

export const instrumentPermissionTone: Record<InstrumentPermission, Tone> = {
  Allowed: 'success',
  Capped: 'warning',
  Forbidden: 'danger',
};

export const mandateTypeLabel: Record<MandateType, string> = {
  Pricing: 'Precificação (fixação)',
  Currency: 'Moeda',
  Commercial: 'Comercial (físico)',
  Logistics: 'Logística (frete)',
};

export const mandateStatusLabel: Record<MandateStatus, string> = {
  PendingApproval: 'Pendente de aprovação',
  Active: 'Ativo',
  Rejected: 'Rejeitado',
  Closed: 'Encerrado',
};

export const mandateStatusTone: Record<MandateStatus, Tone> = {
  PendingApproval: 'warning',
  Active: 'success',
  Rejected: 'danger',
  Closed: 'neutral',
};

export const complianceLabel: Record<ComplianceStatus, string> = {
  Within: 'Dentro da política',
  Outside: 'FORA da política',
};

export const complianceTone: Record<ComplianceStatus, Tone> = {
  Within: 'success',
  Outside: 'danger',
};

export const orderTypeLabel: Record<OrderType, string> = {
  Futures: 'Futuro',
  Option: 'Opção',
  Ndf: 'NDF',
};

export const directionLabel: Record<TradeDirection, string> = {
  Buy: 'Compra',
  Sell: 'Venda',
};

export const optionKindLabel: Record<OptionKind, string> = {
  Call: 'Call',
  Put: 'Put',
};

export const approvalLabel: Record<ApprovalStatus, string> = {
  PendingApproval: 'Pendente de aprovação',
  Approved: 'Aprovada',
  Rejected: 'Rejeitada',
};

export const approvalTone: Record<ApprovalStatus, Tone> = {
  PendingApproval: 'warning',
  Approved: 'success',
  Rejected: 'danger',
};

export const confirmationLabel: Record<ConfirmationStatus, string> = {
  Pending: 'Confirmation pendente',
  Confirmed: 'Confirmado',
  Divergent: 'Divergente',
  Refused: 'Recusado',
};

export const confirmationTone: Record<ConfirmationStatus, Tone> = {
  Pending: 'warning',
  Confirmed: 'success',
  Divergent: 'danger',
  Refused: 'danger',
};

export const timelineActionLabel: Record<TimelineEntry['action'], string> = {
  Created: 'Criação',
  Updated: 'Alteração',
  Deleted: 'Exclusão',
};

export const entityTypeLabel: Record<string, string> = {
  Organization: 'Organização',
  OrganizationGroup: 'Grupo',
  Counterparty: 'Contraparte',
  Policy: 'Política',
  Mandate: 'Mandato',
  Order: 'Boleta',
  Rule: 'Rule',
};

/** Prefixos das entidades filhas registradas na timeline do agregado dono. */
export const childEntityLabel: Record<string, string> = {
  OrganizationMember: 'Membro',
  OrganizationRule: 'Rule do membro',
  OrganizationCommodity: 'Commodity',
  OrganizationGroupMember: 'Membro do grupo',
  PolicyAxis: 'Eixo',
  CoverageBand: 'Banda',
  PolicyInstrument: 'Instrumento',
  PolicyVersion: 'Versão',
  RuleRole: 'Role',
};
