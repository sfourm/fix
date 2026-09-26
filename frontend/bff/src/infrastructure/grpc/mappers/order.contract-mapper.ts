import type { OrderTermsInput } from '../../../application/orders/commands/order-terms.input.js';
import type { OrderDto } from '../../../application/orders/dtos/order.dto.js';
import { nullable, nullableNumber } from './common.contract-mapper.js';
import {
  approvalStatusEnum,
  commodityEnum,
  complianceStatusEnum,
  confirmationStatusEnum,
  optionKindEnum,
  orderTypeEnum,
  tradeDirectionEnum,
} from './enum.contract-mapper.js';

export interface ContractOrderTerms {
  type: string;
  direction: string;
  tenor: string;
  lots?: number;
  notionalUsd?: number;
  price: number;
  priceUnit?: string;
  optionKind: string;
  premium?: number;
  tradeDate: string;
  notes?: string;
  commodity?: string;
  coveredSale?: boolean;
  justification?: string;
}

export interface ContractOrder {
  id: string;
  code: string;
  mandateId?: string;
  mandateCode?: string;
  mandateTitle: string;
  counterpartyId: string;
  counterpartyName: string;
  commodity: string;
  terms: ContractOrderTerms;
  approval: string;
  requestedBy: string;
  decidedBy?: string;
  decidedAt?: string;
  decisionNote?: string;
  confirmation: string;
  confirmedOn?: string;
  confirmationNote?: string;
  confirmationOverdue: boolean;
  confirmationBy?: string;
  compliance: { status: string; reason: string } | null;
  linkedAfterExecution: boolean;
  exceedsMandate: boolean;
  deviationNote?: string;
}

export const toOrderDto = (o: ContractOrder): OrderDto => ({
  id: o.id,
  code: o.code,
  mandateId: nullable(o.mandateId),
  mandateCode: nullable(o.mandateCode),
  mandateTitle: o.mandateTitle,
  counterpartyId: o.counterpartyId,
  counterpartyName: o.counterpartyName,
  commodity: commodityEnum.fromContract(o.commodity),
  terms: {
    type: orderTypeEnum.fromContractRequired(o.terms.type),
    direction: tradeDirectionEnum.fromContractRequired(o.terms.direction),
    tenor: o.terms.tenor,
    lots: nullableNumber(o.terms.lots),
    notionalUsd: nullableNumber(o.terms.notionalUsd),
    price: o.terms.price,
    priceUnit: nullable(o.terms.priceUnit),
    optionKind: optionKindEnum.fromContract(o.terms.optionKind),
    premium: nullableNumber(o.terms.premium),
    tradeDate: o.terms.tradeDate,
    notes: nullable(o.terms.notes),
    coveredSale: o.terms.coveredSale ?? false,
  },
  approval: approvalStatusEnum.fromContractRequired(o.approval),
  requestedBy: o.requestedBy,
  decidedBy: nullable(o.decidedBy),
  decidedAt: nullable(o.decidedAt),
  decisionNote: nullable(o.decisionNote),
  confirmation: confirmationStatusEnum.fromContractRequired(o.confirmation),
  confirmedOn: nullable(o.confirmedOn),
  confirmationNote: nullable(o.confirmationNote),
  confirmationOverdue: o.confirmationOverdue,
  confirmationBy: nullable(o.confirmationBy),
  compliance: {
    status: complianceStatusEnum.fromContract(o.compliance?.status ?? '') === 'Outside' ? 'Outside' : 'Within',
    reason: o.compliance?.reason ?? '',
  },
  linkedAfterExecution: o.linkedAfterExecution,
  exceedsMandate: o.exceedsMandate,
  deviationNote: nullable(o.deviationNote),
});

export const toContractOrderTerms = (t: OrderTermsInput) => ({
  ...t,
  type: orderTypeEnum.toContract(t.type),
  direction: tradeDirectionEnum.toContract(t.direction),
  optionKind: optionKindEnum.toContract(t.optionKind),
  commodity: commodityEnum.toContract(t.commodity),
});
