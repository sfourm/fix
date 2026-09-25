import { Router } from 'express';
import type { TimelineService } from '../../../application/timeline/services/timeline.service.js';
import { organizationContext } from '../request-context.js';
import { parse, schemas } from '../validation.js';

/** /api/timeline */
export function timelineRoutes(timeline: TimelineService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    const filter = parse(schemas.timelineQuery, req.query);
    res.json(await timeline.list({ context: organizationContext(req), ...filter }));
  });

  return router;
}
