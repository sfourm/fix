import { Router } from 'express';
import type { PolicyService } from '../../../application/policies/services/policy.service.js';
import { organizationContext } from '../request-context.js';
import { childParam, idParam, parse, schemas } from '../validation.js';

/** /api/policies: política-mãe versionada e suas partes (eixos, bandas de cobertura e instrumentos). */
export function policyRoutes(policies: PolicyService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    const { page } = parse(schemas.policiesQuery, req.query);
    res.json(await policies.list({ context: organizationContext(req), page }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.createPolicy, req.body);
    res.status(201).json(await policies.create({ context: organizationContext(req), ...body }));
  });

  router.get('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await policies.get({ context: organizationContext(req), id }));
  });

  router.put('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.updatePolicy, req.body);
    res.json(await policies.update({ context: organizationContext(req), id, ...body }));
  });

  router.delete('/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await policies.delete({ context: organizationContext(req), id });
    res.status(204).end();
  });

  router.put('/:id/limits', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const limits = parse(schemas.policyLimits, req.body);
    res.json(await policies.updateLimits({ context: organizationContext(req), id, limits }));
  });

  // ---------- Ciclo de aprovação ----------

  router.post('/:id/submit', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await policies.submit({ context: organizationContext(req), id }));
  });

  router.post('/:id/approve', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { approvalRecord } = parse(schemas.approvePolicy, req.body);
    res.json(await policies.approve({ context: organizationContext(req), id, approvalRecord }));
  });

  router.post('/:id/versions', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.openPolicyVersion, req.body);
    res.json(await policies.openVersion({ context: organizationContext(req), id, ...body }));
  });

  // ---------- Eixos ----------

  router.post('/:id/axes', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const axis = parse(schemas.policyAxis, req.body);
    res.status(201).json(await policies.addAxis({ context: organizationContext(req), policyId: id, axis }));
  });

  router.put('/:id/axes/:childId', async (req, res) => {
    const { id, childId } = parse(childParam, req.params);
    const axis = parse(schemas.policyAxis, req.body);
    res.json(await policies.updateAxis({ context: organizationContext(req), policyId: id, axisId: childId, axis }));
  });

  router.delete('/:id/axes/:childId', async (req, res) => {
    const { id, childId } = parse(childParam, req.params);
    res.json(await policies.removeAxis({ context: organizationContext(req), policyId: id, axisId: childId }));
  });

  // ---------- Bandas de cobertura ----------

  router.post('/:id/bands', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const band = parse(schemas.coverageBand, req.body);
    res.status(201).json(await policies.addBand({ context: organizationContext(req), policyId: id, band }));
  });

  router.put('/:id/bands/:childId', async (req, res) => {
    const { id, childId } = parse(childParam, req.params);
    const band = parse(schemas.coverageBand, req.body);
    res.json(await policies.updateBand({ context: organizationContext(req), policyId: id, bandId: childId, band }));
  });

  router.delete('/:id/bands/:childId', async (req, res) => {
    const { id, childId } = parse(childParam, req.params);
    res.json(await policies.removeBand({ context: organizationContext(req), policyId: id, bandId: childId }));
  });

  // ---------- Instrumentos ----------

  router.post('/:id/instruments', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const instrument = parse(schemas.policyInstrument, req.body);
    res.status(201).json(await policies.addInstrument({ context: organizationContext(req), policyId: id, instrument }));
  });

  router.put('/:id/instruments/:childId', async (req, res) => {
    const { id, childId } = parse(childParam, req.params);
    const instrument = parse(schemas.policyInstrument, req.body);
    res.json(
      await policies.updateInstrument({ context: organizationContext(req), policyId: id, instrumentId: childId, instrument }),
    );
  });

  router.delete('/:id/instruments/:childId', async (req, res) => {
    const { id, childId } = parse(childParam, req.params);
    res.json(await policies.removeInstrument({ context: organizationContext(req), policyId: id, instrumentId: childId }));
  });

  return router;
}
