import { Router } from 'express';
import type { CounterpartyService } from '../../../application/counterparties/services/counterparty.service.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse, schemas } from '../validation.js';

/** /api/counterparties */
export function counterpartyRoutes(counterparties: CounterpartyService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    const { onlyHomologated } = parse(schemas.counterpartiesQuery, req.query);
    res.json(await counterparties.list({ context: organizationContext(req), onlyHomologated }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.counterparty, req.body);
    res.status(201).json(await counterparties.create({ context: organizationContext(req), ...body }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await counterparties.get({ context: organizationContext(req), id }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.counterparty, req.body);
    res.json(await counterparties.update({ context: organizationContext(req), id, ...body }));
  });

  router.patch('/:id/homologation', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { homologated } = parse(schemas.homologation, req.body);
    res.json(await counterparties.setHomologation({ context: organizationContext(req), id, homologated }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await counterparties.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  return router;
}
