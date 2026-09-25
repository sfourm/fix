import { Router } from 'express';
import type { RuleService } from '../../../application/rules/services/rule.service.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse, schemas } from '../validation.js';

/** /api/rules: rules da organização (owner, user) e as alçadas personalizadas que ela cria e edita. */
export function ruleRoutes(rules: RuleService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    res.json(await rules.list({ context: organizationContext(req) }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.rule, req.body);
    res.status(201).json(await rules.create({ context: organizationContext(req), ...body }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.rule, req.body);
    res.json(await rules.update({ context: organizationContext(req), id, ...body }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await rules.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  return router;
}
