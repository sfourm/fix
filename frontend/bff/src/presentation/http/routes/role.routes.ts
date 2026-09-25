import { Router } from 'express';
import type { RoleService } from '../../../application/roles/services/role.service.js';
import { userContext } from '../request-context.js';

/** /api/roles: permissões atômicas (catálogo do sistema). */
export function roleRoutes(roles: RoleService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    res.json(await roles.list({ context: userContext(req) }));
  });

  return router;
}
