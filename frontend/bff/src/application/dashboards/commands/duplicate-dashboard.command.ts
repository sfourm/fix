import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Cria uma cópia privada (ex.: para personalizar um dashboard público da organização). */
export interface DuplicateDashboardCommand {
  context: RequestContext;
  id: string;
  name: string | null;
}
