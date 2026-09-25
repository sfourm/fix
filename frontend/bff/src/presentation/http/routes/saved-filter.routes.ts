import { Router } from 'express';
import type { SavedFilterService } from '../../../application/filters/services/saved-filter.service.js';
import { dashboardSchemas as schemas } from '../dashboard.validation.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse } from '../validation.js';

/** /api/filters: filtros salvos (privados ou públicos na organização). */
export function savedFilterRoutes(filters: SavedFilterService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    const { source } = parse(schemas.savedFiltersQuery, req.query);
    res.json(await filters.list({ context: organizationContext(req), source: source ?? null }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.savedFilter, req.body);
    res.status(201).json(await filters.create({ context: organizationContext(req), ...body }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await filters.get({ context: organizationContext(req), id }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.updateSavedFilter, req.body);
    res.json(await filters.update({ context: organizationContext(req), id, ...body }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await filters.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  return router;
}
