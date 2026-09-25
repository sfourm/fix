import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface CreateGroupCommand {
  context: RequestContext;
  name: string;
  ruleCodes: string[];
}
