import type { OrderTermsInput } from './order-terms.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RegisterOrderCommand {
  context: RequestContext;
  /** Nulo = boleta sem mandato (exige justificativa nos termos). */
  mandateId: string | null;
  counterpartyId: string;
  terms: OrderTermsInput;
}
