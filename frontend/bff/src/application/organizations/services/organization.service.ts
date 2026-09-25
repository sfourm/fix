import type { AddCommodityCommand } from '../commands/add-commodity.command.js';
import type { AddGroupMemberCommand } from '../commands/add-group-member.command.js';
import type { AddMemberCommand } from '../commands/add-member.command.js';
import type { ChangeMemberDeskCommand } from '../commands/change-member-desk.command.js';
import type { CreateGroupCommand } from '../commands/create-group.command.js';
import type { CreateOrganizationCommand } from '../commands/create-organization.command.js';
import type { RemoveCommodityCommand } from '../commands/remove-commodity.command.js';
import type { RemoveMemberCommand } from '../commands/remove-member.command.js';
import type { UpdateBudgetCommand } from '../commands/update-budget.command.js';
import type { UpdateCommodityCommand } from '../commands/update-commodity.command.js';
import type { UpdateCompanyProfileCommand } from '../commands/update-company-profile.command.js';
import type { UpdateFinancialsCommand } from '../commands/update-financials.command.js';
import type { UpdateIndustrialProfileCommand } from '../commands/update-industrial-profile.command.js';
import type { UpdateOrganizationCommand } from '../commands/update-organization.command.js';
import {
  toGroupResponse,
  toMemberResponse,
  toOrganizationResponse,
  toOrganizationSetupResponse,
} from '../mappers/organization.mapper.js';
import type { OrganizationGateway } from '../ports/organization.gateway.js';
import type { GetOrganizationQuery } from '../queries/get-organization.query.js';
import type { GetUserRolesQuery } from '../queries/get-user-roles.query.js';
import type { ListGroupsQuery } from '../queries/list-groups.query.js';
import type { ListMembersQuery } from '../queries/list-members.query.js';
import type { ListUserOrganizationsQuery } from '../queries/list-user-organizations.query.js';
import type { GroupResponse } from '../responses/group.response.js';
import type { MemberResponse } from '../responses/member.response.js';
import type { OrganizationSetupResponse } from '../responses/organization-setup.response.js';
import type { OrganizationResponse } from '../responses/organization.response.js';
import type { UserRolesResponse } from '../responses/user-roles.response.js';

export class OrganizationService {
  constructor(private readonly gateway: OrganizationGateway) {}

  // ---------- Organização e setup ----------

  async create(command: CreateOrganizationCommand): Promise<OrganizationResponse> {
    return toOrganizationResponse(await this.gateway.create(command));
  }

  async update(command: UpdateOrganizationCommand): Promise<OrganizationResponse> {
    return toOrganizationResponse(await this.gateway.update(command));
  }

  async updateProfile(command: UpdateCompanyProfileCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.updateProfile(command));
  }

  async updateIndustrial(command: UpdateIndustrialProfileCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.updateIndustrial(command));
  }

  async updateBudget(command: UpdateBudgetCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.updateBudget(command));
  }

  async updateFinancials(command: UpdateFinancialsCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.updateFinancials(command));
  }

  async addCommodity(command: AddCommodityCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.addCommodity(command));
  }

  async updateCommodity(command: UpdateCommodityCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.updateCommodity(command));
  }

  async removeCommodity(command: RemoveCommodityCommand): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.removeCommodity(command));
  }

  // ---------- Membros e grupos ----------

  /** Adiciona o membro e devolve a lista atualizada (o web renderiza direto). */
  async addMember(command: AddMemberCommand): Promise<MemberResponse[]> {
    await this.gateway.addMember(command);
    return this.listMembers({ context: command.context });
  }

  async changeMemberDesk(command: ChangeMemberDeskCommand): Promise<MemberResponse[]> {
    await this.gateway.changeMemberDesk(command);
    return this.listMembers({ context: command.context });
  }

  async removeMember(command: RemoveMemberCommand): Promise<void> {
    await this.gateway.removeMember(command);
  }

  async createGroup(command: CreateGroupCommand): Promise<GroupResponse[]> {
    await this.gateway.createGroup(command);
    return this.listGroups({ context: command.context });
  }

  async addGroupMember(command: AddGroupMemberCommand): Promise<GroupResponse[]> {
    await this.gateway.addGroupMember(command);
    return this.listGroups({ context: command.context });
  }

  // ---------- Queries ----------

  async listForUser(query: ListUserOrganizationsQuery): Promise<OrganizationResponse[]> {
    return (await this.gateway.listForUser(query)).map(toOrganizationResponse);
  }

  async get(query: GetOrganizationQuery): Promise<OrganizationSetupResponse> {
    return toOrganizationSetupResponse(await this.gateway.get(query));
  }

  async getUserRoles(query: GetUserRolesQuery): Promise<UserRolesResponse> {
    return [...(await this.gateway.getUserRoles(query))].sort();
  }

  async listMembers(query: ListMembersQuery): Promise<MemberResponse[]> {
    return (await this.gateway.listMembers(query)).map(toMemberResponse);
  }

  async listGroups(query: ListGroupsQuery): Promise<GroupResponse[]> {
    return (await this.gateway.listGroups(query)).map(toGroupResponse);
  }
}
