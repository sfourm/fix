import { Router } from 'express';
import type { DashboardService } from '../../../application/dashboards/services/dashboard.service.js';
import { dashboardSchemas as schemas } from '../dashboard.validation.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse } from '../validation.js';

/** /api/dashboards: dashboards do usuário e públicos da organização (armazenados no BFF). */
export function dashboardRoutes(dashboards: DashboardService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    res.json(await dashboards.list({ context: organizationContext(req) }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.createDashboard, req.body);
    res.status(201).json(await dashboards.create({ context: organizationContext(req), ...body }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await dashboards.get({ context: organizationContext(req), id }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.updateDashboard, req.body);
    res.json(await dashboards.update({ context: organizationContext(req), id, ...body }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await dashboards.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  router.post('/:id/duplicate', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { name } = parse(schemas.duplicateDashboard, req.body ?? {});
    res.status(201).json(await dashboards.duplicate({ context: organizationContext(req), id, name }));
  });

  // ---------- Widgets ----------

  router.post('/:id/widgets', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const widget = parse(schemas.widget, req.body);
    res.status(201).json(await dashboards.addWidget({ context: organizationContext(req), dashboardId: id, widget }));
  });

  router.put('/:id/widgets/order', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { widgetIds } = parse(schemas.reorderWidgets, req.body);
    res.json(await dashboards.reorderWidgets({ context: organizationContext(req), dashboardId: id, widgetIds }));
  });

  router.put('/:id/widgets/:widgetId', async (req, res) => {
    const { id, widgetId } = parse(schemas.widgetParams, req.params);
    const widget = parse(schemas.widget, req.body);
    res.json(await dashboards.updateWidget({ context: organizationContext(req), dashboardId: id, widgetId, widget }));
  });

  router.delete('/:id/widgets/:widgetId', async (req, res) => {
    const { id, widgetId } = parse(schemas.widgetParams, req.params);
    res.json(await dashboards.removeWidget({ context: organizationContext(req), dashboardId: id, widgetId }));
  });

  router.get('/:id/widgets/:widgetId/data', async (req, res) => {
    const { id, widgetId } = parse(schemas.widgetParams, req.params);
    res.json(await dashboards.widgetData({ context: organizationContext(req), dashboardId: id, widgetId }));
  });

  return router;
}
