import type { OrderTermsInput } from './order-terms.input.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateOrderCommand {
  context: RequestContext;
  id: string;
  counterpartyId: string;
  terms: OrderTermsInput;
}
