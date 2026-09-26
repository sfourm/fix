import { Router } from 'express';
import type { TimelineService } from '../../../application/timeline/services/timeline.service.js';
import { organizationContext } from '../request-context.js';
import { parse, schemas } from '../validation.js';

/** /api/timeline (auditoria) */
export function timelineRoutes(timeline: TimelineService): Router {
  const router = Router();

  /** Os mais recentes (até `limit`): históricos embutidos nas telas. */
  router.get('/', async (req, res) => {
    const filter = parse(schemas.timelineQuery, req.query);
    res.json(await timeline.list({ context: organizationContext(req), ...filter }));
  });

  /** Paginado com o total: tela de Auditoria e exportação. */
  router.get('/page', async (req, res) => {
    const query = parse(schemas.timelinePageQuery, req.query);
    res.json(await timeline.page({ context: organizationContext(req), ...query }));
  });

  return router;
}
