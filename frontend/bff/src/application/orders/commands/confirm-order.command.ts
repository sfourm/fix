import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface ConfirmOrderCommand {
  context: RequestContext;
  id: string;
  /** Data de recebimento da confirmação. */
  receivedOn: string;
}
