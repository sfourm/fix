import type { ApprovalStatus } from '../../../cross-cutting/enums/approval-status.js';
import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { ConfirmationStatus } from '../../../cross-cutting/enums/confirmation-status.js';
import type { OrderTermsResponse } from './order-terms.response.js';

export interface OrderResponse {
  id: string;
  /** Código legível da organização (HX-0001). */
  code: string;
  /** Nulo = boleta sem mandato (desvio sinalizado, FIX2 · I-01). */
  mandateId: string | null;
  /** MD-01 do mandato, quando houver. */
  mandateCode: string | null;
  mandateTitle: string;
  counterpartyId: string;
  counterpartyName: string;
  /** Ausente em NDF (hedge de moeda, sem commodity). */
  commodity: Commodity | null;
  terms: OrderTermsResponse;
  approval: ApprovalStatus;
  requestedBy: string;
  decidedBy: string | null;
  decidedAt: string | null;
  decisionNote: string | null;
  /** Nasce pendente; só pode ser tratado pelo middle office depois da aprovação. */
  confirmation: ConfirmationStatus;
  confirmedOn: string | null;
  confirmationNote: string | null;
  /** Pendente há mais de 2 dias úteis. */
  confirmationOverdue: boolean;
  /** Middle office que registrou a confirmação (nunca quem executou). */
  confirmationBy: string | null;
  /** Enquadramento calculado da boleta (dentro/FORA e o motivo). */
  compliance: { status: 'Within' | 'Outside'; reason: string };
  /** Mandato vinculado depois da execução: carimbo permanente. */
  linkedAfterExecution: boolean;
  /** Estourou o saldo do mandato. */
  exceedsMandate: boolean;
  /** Justificativa do desvio. */
  deviationNote: string | null;
}
