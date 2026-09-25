import type { RequestContext } from '../../../cross-cutting/context/request-context.js';

/** Organograma: o grupo (com os grupos abaixo dele) passa a ficar abaixo de parentGroupId. */
export interface MoveGroupCommand {
  context: RequestContext;
  groupId: string;
  parentGroupId: string;
}
