import type { AddCommodityCommand } from '../../../application/organizations/commands/add-commodity.command.js';
import type { AddGroupMemberCommand } from '../../../application/organizations/commands/add-group-member.command.js';
import type { AddMemberCommand } from '../../../application/organizations/commands/add-member.command.js';
import type { ChangeMemberDeskCommand } from '../../../application/organizations/commands/change-member-desk.command.js';
import type { CreateGroupCommand } from '../../../application/organizations/commands/create-group.command.js';
import type { CreateOrganizationCommand } from '../../../application/organizations/commands/create-organization.command.js';
import type { RemoveCommodityCommand } from '../../../application/organizations/commands/remove-commodity.command.js';
import type { RemoveMemberCommand } from '../../../application/organizations/commands/remove-member.command.js';
import type { UpdateBudgetCommand } from '../../../application/organizations/commands/update-budget.command.js';
import type { UpdateCommodityCommand } from '../../../application/organizations/commands/update-commodity.command.js';
import type { UpdateCompanyProfileCommand } from '../../../application/organizations/commands/update-company-profile.command.js';
import type { UpdateFinancialsCommand } from '../../../application/organizations/commands/update-financials.command.js';
import type { UpdateIndustrialProfileCommand } from '../../../application/organizations/commands/update-industrial-profile.command.js';
import type { UpdateOrganizationCommand } from '../../../application/organizations/commands/update-organization.command.js';
import type { GroupDto } from '../../../application/organizations/dtos/group.dto.js';
import type { MemberDto } from '../../../application/organizations/dtos/member.dto.js';
import type { OrganizationSetupDto } from '../../../application/organizations/dtos/organization-setup.dto.js';
import type { OrganizationDto } from '../../../application/organizations/dtos/organization.dto.js';
import type { OrganizationGateway } from '../../../application/organizations/ports/organization.gateway.js';
import type { GetOrganizationQuery } from '../../../application/organizations/queries/get-organization.query.js';
import type { GetUserRolesQuery } from '../../../application/organizations/queries/get-user-roles.query.js';
import type { ListGroupsQuery } from '../../../application/organizations/queries/list-groups.query.js';
import type { ListMembersQuery } from '../../../application/organizations/queries/list-members.query.js';
import type { ListUserOrganizationsQuery } from '../../../application/organizations/queries/list-user-organizations.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext } from '../mappers/common.contract-mapper.js';
import { commodityEnum, deskEnum } from '../mappers/enum.contract-mapper.js';
import {
  toContractBudget,
  toContractCommodityInput,
  toContractFinancials,
  toContractIndustrial,
  toContractProfile,
  toGroupDto,
  toMemberDto,
  toOrganizationDto,
  toOrganizationSetupDto,
  type ContractGroup,
  type ContractMember,
  type ContractOrganization,
  type ContractOrganizationSetup,
} from '../mappers/organization.contract-mapper.js';

const SERVICE = 'OrganizationService';

export class GrpcOrganizationGateway implements OrganizationGateway {
  constructor(private readonly core: CoreClient) {}

  // ---------- Organização e setup ----------

  async create(command: CreateOrganizationCommand): Promise<OrganizationDto> {
    return toOrganizationDto(await this.call<ContractOrganization>('CreateOrganization', command.context, { name: command.name }));
  }

  async update(command: UpdateOrganizationCommand): Promise<OrganizationDto> {
    return toOrganizationDto(await this.call<ContractOrganization>('UpdateOrganization', command.context, { name: command.name }));
  }

  updateProfile(command: UpdateCompanyProfileCommand): Promise<OrganizationSetupDto> {
    return this.setup('UpdateCompanyProfile', command.context, { profile: toContractProfile(command) });
  }

  updateIndustrial(command: UpdateIndustrialProfileCommand): Promise<OrganizationSetupDto> {
    return this.setup('UpdateIndustrialProfile', command.context, { industrial: toContractIndustrial(command) });
  }

  updateBudget(command: UpdateBudgetCommand): Promise<OrganizationSetupDto> {
    return this.setup('UpdateBudget', command.context, { budget: toContractBudget(command) });
  }

  updateFinancials(command: UpdateFinancialsCommand): Promise<OrganizationSetupDto> {
    return this.setup('UpdateFinancials', command.context, { financials: toContractFinancials(command) });
  }

  addCommodity(command: AddCommodityCommand): Promise<OrganizationSetupDto> {
    return this.setup('AddCommodity', command.context, {
      commodity: commodityEnum.toContract(command.commodity),
      data: toContractCommodityInput(command),
    });
  }

  updateCommodity(command: UpdateCommodityCommand): Promise<OrganizationSetupDto> {
    return this.setup('UpdateCommodity', command.context, {
      commodityId: command.commodityId,
      data: toContractCommodityInput(command),
    });
  }

  removeCommodity(command: RemoveCommodityCommand): Promise<OrganizationSetupDto> {
    return this.setup('RemoveCommodity', command.context, { commodityId: command.commodityId });
  }

  // ---------- Membros e grupos ----------

  async addMember(command: AddMemberCommand): Promise<string> {
    const response = await this.call<{ memberId: string }>('AddMember', command.context, {
      email: command.email,
      ruleCode: command.ruleCode,
      desk: deskEnum.toContract(command.desk),
    });
    return response.memberId;
  }

  async changeMemberDesk(command: ChangeMemberDeskCommand): Promise<void> {
    await this.call('ChangeMemberDesk', command.context, {
      memberId: command.memberId,
      desk: deskEnum.toContract(command.desk),
    });
  }

  async removeMember(command: RemoveMemberCommand): Promise<void> {
    await this.call('RemoveMember', command.context, { memberId: command.memberId });
  }

  async createGroup(command: CreateGroupCommand): Promise<string> {
    const response = await this.call<{ groupId: string }>('CreateGroup', command.context, {
      name: command.name,
      ruleCodes: command.ruleCodes,
    });
    return response.groupId;
  }

  async addGroupMember(command: AddGroupMemberCommand): Promise<void> {
    await this.call('AddGroupMember', command.context, { groupId: command.groupId, memberId: command.memberId });
  }

  // ---------- Queries ----------

  async listForUser(query: ListUserOrganizationsQuery): Promise<OrganizationDto[]> {
    const response = await this.call<{ organizations: ContractOrganization[] }>('ListUserOrganizations', query.context);
    return response.organizations.map(toOrganizationDto);
  }

  get(query: GetOrganizationQuery): Promise<OrganizationSetupDto> {
    return this.setup('GetOrganization', query.context);
  }

  async getUserRoles(query: GetUserRolesQuery): Promise<string[]> {
    return (await this.call<{ roles: string[] }>('GetUserRoles', query.context)).roles;
  }

  async listMembers(query: ListMembersQuery): Promise<MemberDto[]> {
    return (await this.call<{ members: ContractMember[] }>('ListMembers', query.context)).members.map(toMemberDto);
  }

  async listGroups(query: ListGroupsQuery): Promise<GroupDto[]> {
    return (await this.call<{ groups: ContractGroup[] }>('ListGroups', query.context)).groups.map(toGroupDto);
  }

  private async setup(method: string, context: RequestContext, request: object = {}): Promise<OrganizationSetupDto> {
    return toOrganizationSetupDto(await this.call<ContractOrganizationSetup>(method, context, request));
  }

  private call<T>(method: string, context: RequestContext, request: object = {}): Promise<T> {
    return this.core.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
