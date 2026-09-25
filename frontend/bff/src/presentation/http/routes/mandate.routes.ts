import { Router } from 'express';
import type { MandateService } from '../../../application/mandates/services/mandate.service.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse, schemas } from '../validation.js';

/** /api/mandates */
export function mandateRoutes(mandates: MandateService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    const filter = parse(schemas.mandatesQuery, req.query);
    res.json(await mandates.list({ context: organizationContext(req), ...filter }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.issueMandate, req.body);
    res.status(201).json(await mandates.issue({ context: organizationContext(req), ...body }));
  });

  /** Checagem de aderência à política antes de emitir (não grava nada). */
  router.post('/preview', async (req, res) => {
    const body = parse(schemas.issueMandate, req.body);
    res.json(await mandates.previewCompliance({ context: organizationContext(req), ...body }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await mandates.get({ context: organizationContext(req), id }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { terms } = parse(schemas.updateMandate, req.body);
    res.json(await mandates.update({ context: organizationContext(req), id, terms }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await mandates.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  router.post('/:id/approve', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.note, req.body ?? {});
    res.json(await mandates.approve({ context: organizationContext(req), id, note }));
  });

  router.post('/:id/reject', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.requiredNote, req.body);
    res.json(await mandates.reject({ context: organizationContext(req), id, note }));
  });

  router.post('/:id/close', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.note, req.body ?? {});
    res.json(await mandates.close({ context: organizationContext(req), id, note }));
  });

  return router;
}
