import type { OrderTermsInput } from './order-terms.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface RegisterOrderCommand {
  context: RequestContext;
  mandateId: string;
  counterpartyId: string;
  terms: OrderTermsInput;
}
