import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

export interface UpdateOrganizationCommand {
  context: RequestContext;
  name: string;
}
