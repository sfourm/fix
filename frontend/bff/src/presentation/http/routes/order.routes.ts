import { Router } from 'express';
import type { OrderService } from '../../../application/orders/services/order.service.js';
import { organizationContext } from '../request-context.js';
import { idParam, parse, schemas } from '../validation.js';

/** /api/orders: boletas de hedge. */
export function orderRoutes(orders: OrderService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    const filter = parse(schemas.ordersQuery, req.query);
    res.json(await orders.list({ context: organizationContext(req), ...filter }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.registerOrder, req.body);
    res.status(201).json(await orders.register({ context: organizationContext(req), ...body }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await orders.get({ context: organizationContext(req), id }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.updateOrder, req.body);
    res.json(await orders.update({ context: organizationContext(req), id, ...body }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await orders.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  /** Vínculo a posteriori (boleta sem mandato): exige justificativa e deixa o carimbo para sempre. */
  router.post('/:id/link', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.linkOrderMandate, req.body);
    res.json(await orders.link({ context: organizationContext(req), id, ...body }));
  });

  // ---------- Aprovação ----------

  router.post('/:id/approve', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.note, req.body ?? {});
    res.json(await orders.approve({ context: organizationContext(req), id, note }));
  });

  router.post('/:id/reject', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.requiredNote, req.body);
    res.json(await orders.reject({ context: organizationContext(req), id, note }));
  });

  // ---------- Confirmação (middle office) ----------

  router.post('/:id/confirmation', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { receivedOn } = parse(schemas.confirmOrder, req.body);
    res.json(await orders.confirm({ context: organizationContext(req), id, receivedOn }));
  });

  router.post('/:id/divergence', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.requiredNote, req.body);
    res.json(await orders.markDivergent({ context: organizationContext(req), id, note }));
  });

  router.post('/:id/refusal', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { note } = parse(schemas.requiredNote, req.body);
    res.json(await orders.refuseConfirmation({ context: organizationContext(req), id, note }));
  });

  router.post('/:id/resolve', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await orders.resolveDivergence({ context: organizationContext(req), id }));
  });

  return router;
}
