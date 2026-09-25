import { Router } from 'express';
import type { RuleService } from '../../../application/rules/services/rule.service.js';
import { userContext } from '../request-context.js';

/** /api/rules: conjuntos de roles atribuíveis a membros e grupos. */
export function ruleRoutes(rules: RuleService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    res.json(await rules.list({ context: userContext(req) }));
  });

  return router;
}
