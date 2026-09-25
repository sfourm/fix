import { Router } from 'express';
import type { SearchService } from '../../../application/search/services/search.service.js';
import { dashboardSchemas as schemas } from '../dashboard.validation.js';
import { organizationContext } from '../request-context.js';
import { parse } from '../validation.js';

/** /api/search: catálogo de conjuntos de dados, pesquisa com filtros e prévia de widgets. */
export function searchRoutes(search: SearchService): Router {
  const router = Router();

  router.get('/sources', async (req, res) => {
    res.json(await search.listDataSources({ context: organizationContext(req) }));
  });

  /** Prévia do editor de widgets (não grava nada). */
  router.post('/aggregate', async (req, res) => {
    const { type, query } = parse(schemas.widgetPreview, req.body);
    res.json(await search.widgetData({ context: organizationContext(req), type, query }));
  });

  router.post('/:source', async (req, res) => {
    const { source } = parse(schemas.sourceParam, req.params);
    const { page, pageSize, ...body } = parse(schemas.search, req.body ?? {});
    res.json(await search.search({ context: organizationContext(req), source, ...body, page: { page, pageSize } }));
  });

  return router;
}
