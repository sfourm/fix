import { Router } from 'express';
import type { OrganizationService } from '../../../application/organizations/services/organization.service.js';
import { organizationContext, userContext } from '../request-context.js';
import { idParam, parse, schemas } from '../validation.js';

/** /api/organizations: organizações do usuário (sem tenant). */
export function organizationsRoutes(organizations: OrganizationService): Router {
  const router = Router();

  router.get('/', async (req, res) => {
    res.json(await organizations.listForUser({ context: userContext(req) }));
  });

  router.post('/', async (req, res) => {
    const body = parse(schemas.organization, req.body);
    res.status(201).json(await organizations.create({ context: userContext(req), ...body }));
  });

  return router;
}

/** /api/organization: a organização do header X-Organization-Id (setup, membros e grupos). */
export function currentOrganizationRoutes(organizations: OrganizationService): Router {
  const router = Router();

  // ---------- Setup ----------

  router.get('/', async (req, res) => {
    res.json(await organizations.get({ context: organizationContext(req) }));
  });

  router.put('/', async (req, res) => {
    const body = parse(schemas.organization, req.body);
    res.json(await organizations.update({ context: organizationContext(req), ...body }));
  });

  router.put('/profile', async (req, res) => {
    const body = parse(schemas.companyProfile, req.body);
    res.json(await organizations.updateProfile({ context: organizationContext(req), ...body }));
  });

  router.put('/industrial', async (req, res) => {
    const body = parse(schemas.industrialProfile, req.body);
    res.json(await organizations.updateIndustrial({ context: organizationContext(req), ...body }));
  });

  router.put('/budget', async (req, res) => {
    const body = parse(schemas.budget, req.body);
    res.json(await organizations.updateBudget({ context: organizationContext(req), ...body }));
  });

  router.put('/financials', async (req, res) => {
    const body = parse(schemas.financials, req.body);
    res.json(await organizations.updateFinancials({ context: organizationContext(req), ...body }));
  });

  router.post('/commodities', async (req, res) => {
    const body = parse(schemas.addCommodity, req.body);
    res.status(201).json(await organizations.addCommodity({ context: organizationContext(req), ...body }));
  });

  router.put('/commodities/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const body = parse(schemas.updateCommodity, req.body);
    res.json(await organizations.updateCommodity({ context: organizationContext(req), commodityId: id, ...body }));
  });

  router.delete('/commodities/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await organizations.removeCommodity({ context: organizationContext(req), commodityId: id }));
  });

  /** Roles efetivas (alçadas) do usuário nesta organização: o web decide o que exibir. */
  router.get('/roles', async (req, res) => {
    res.json(await organizations.getUserRoles({ context: organizationContext(req) }));
  });

  // ---------- Membros ----------

  router.get('/members', async (req, res) => {
    res.json(await organizations.listMembers({ context: organizationContext(req) }));
  });

  router.post('/members', async (req, res) => {
    const body = parse(schemas.addMember, req.body);
    res.status(201).json(await organizations.addMember({ context: organizationContext(req), ...body }));
  });

  router.patch('/members/:id/desk', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { desk } = parse(schemas.memberDesk, req.body);
    res.json(await organizations.changeMemberDesk({ context: organizationContext(req), memberId: id, desk }));
  });

  /** Alçadas do membro (substitui as atribuídas diretamente; a base owner/user não muda). */
  router.put('/members/:id/rules', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { ruleCodes } = parse(schemas.ruleCodes, req.body);
    res.json(await organizations.setMemberRules({ context: organizationContext(req), memberId: id, ruleCodes }));
  });

  /** Owner: transfere a propriedade (o anterior vira user). */
  router.post('/owner', async (req, res) => {
    const { memberId } = parse(schemas.transferOwnership, req.body);
    await organizations.transferOwnership({ context: organizationContext(req), memberId });
    res.status(204).end();
  });

  router.delete('/members/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    await organizations.removeMember({ context: organizationContext(req), memberId: id });
    res.status(204).end();
  });

  // ---------- Grupos ----------

  router.get('/groups', async (req, res) => {
    res.json(await organizations.listGroups({ context: organizationContext(req) }));
  });

  router.post('/groups', async (req, res) => {
    const body = parse(schemas.createGroup, req.body);
    res.status(201).json(await organizations.createGroup({ context: organizationContext(req), ...body }));
  });

  /** Organograma: muda o grupo pai (a organização mantém a própria hierarquia). */
  router.patch('/groups/:id/parent', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { parentGroupId } = parse(schemas.moveGroup, req.body);
    res.json(await organizations.moveGroup({ context: organizationContext(req), groupId: id, parentGroupId }));
  });

  router.put('/groups/:id/rules', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { ruleCodes } = parse(schemas.ruleCodes, req.body);
    res.json(await organizations.setGroupRules({ context: organizationContext(req), groupId: id, ruleCodes }));
  });

  router.post('/groups/:id/members', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { memberId } = parse(schemas.addGroupMember, req.body);
    res.json(await organizations.addGroupMember({ context: organizationContext(req), groupId: id, memberId }));
  });

  router.delete('/groups/:id/members/:memberId', async (req, res) => {
    const { id, memberId } = parse(schemas.groupMemberParams, req.params);
    res.json(await organizations.removeGroupMember({ context: organizationContext(req), groupId: id, memberId }));
  });

  router.patch('/groups/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    const { name } = parse(schemas.renameGroup, req.body);
    res.json(await organizations.renameGroup({ context: organizationContext(req), groupId: id, name }));
  });

  router.delete('/groups/:id', async (req, res) => {
    const { id } = parse(idParam, req.params);
    res.json(await organizations.deleteGroup({ context: organizationContext(req), groupId: id }));
  });

  return router;
}
