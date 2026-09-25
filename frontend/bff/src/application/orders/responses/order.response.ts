import type { ApprovalStatus } from '../../../cross-cutting/enums/approval-status.js';
import type { Commodity } from '../../../cross-cutting/enums/commodity.js';
import type { ConfirmationStatus } from '../../../cross-cutting/enums/confirmation-status.js';
import type { OrderTermsResponse } from './order-terms.response.js';

export interface OrderResponse {
  id: string;
  mandateId: string;
  mandateTitle: string;
  counterpartyId: string;
  counterpartyName: string;
  commodity: Commodity;
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
}
