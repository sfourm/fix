import type { RequestContext } from '../../cross-cutting/context/request-context.js';
import { AppError } from '../../cross-cutting/errors/app-error.js';
import type { Actor } from '../../domain/common/actor.js';
import type { OrganizationGateway } from '../organizations/ports/organization.gateway.js';
import type { VisualizationCache } from './visualization-cache.js';

const EDIT_ORGANIZATION = 'edit_organization';
const ROLES_TTL_SECONDS = 60;

/**
 * Os recursos do BFF (dashboards, filtros) não passam pelo core, então a membership é conferida aqui:
 * o core só devolve as roles de quem é membro da organização (senão responde 403).
 */
export class ActorResolver {
  constructor(
    private readonly organizations: OrganizationGateway,
    private readonly cache: VisualizationCache,
  ) {}

  async resolve(context: RequestContext): Promise<Actor> {
    const organizationId = context.organizationId;
    if (!organizationId) {
      throw AppError.validation('Informe a organização no header X-Organization-Id.');
    }

    const { value: roles } = await this.cache.remember(
      organizationId,
      `roles:${context.userId}`,
      () => this.organizations.getUserRoles({ context }),
      ROLES_TTL_SECONDS,
    );

    return { userId: context.userId, organizationId, canManageShared: roles.includes(EDIT_ORGANIZATION) };
  }
}
