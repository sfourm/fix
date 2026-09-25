import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Transfere a propriedade da organização (owner atual ou equipe interna FIX). */
export interface TransferOwnershipCommand {
  context: RequestContext;
  memberId: string;
}
