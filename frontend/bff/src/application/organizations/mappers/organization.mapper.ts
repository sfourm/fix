import type { CommodityDto } from '../dtos/commodity.dto.js';
import type { GroupDto } from '../dtos/group.dto.js';
import type { MemberDto } from '../dtos/member.dto.js';
import type { OrganizationSetupDto } from '../dtos/organization-setup.dto.js';
import type { OrganizationDto } from '../dtos/organization.dto.js';
import type { CommodityResponse } from '../responses/commodity.response.js';
import type { GroupResponse } from '../responses/group.response.js';
import type { MemberResponse } from '../responses/member.response.js';
import type { OrganizationSetupResponse } from '../responses/organization-setup.response.js';
import type { OrganizationResponse } from '../responses/organization.response.js';

export const toOrganizationResponse = (dto: OrganizationDto): OrganizationResponse => ({
  id: dto.id,
  name: dto.name,
  slug: dto.slug,
});

export const toCommodityResponse = (dto: CommodityDto): CommodityResponse => ({ ...dto });

export const toOrganizationSetupResponse = (dto: OrganizationSetupDto): OrganizationSetupResponse => ({
  id: dto.id,
  name: dto.name,
  slug: dto.slug,
  profile: { ...dto.profile },
  industrial: { ...dto.industrial },
  budget: { ...dto.budget },
  financials: { ...dto.financials },
  commodities: dto.commodities.map(toCommodityResponse),
});

export const toMemberResponse = (dto: MemberDto): MemberResponse => ({ ...dto, rules: [...dto.rules], groups: [...dto.groups] });

export const toGroupResponse = (dto: GroupDto): GroupResponse => ({ ...dto, rules: [...dto.rules], memberIds: [...dto.memberIds] });
