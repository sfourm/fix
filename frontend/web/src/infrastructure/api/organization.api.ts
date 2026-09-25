import type { Commodity, MeasurementUnit } from '@/domain/common';
import type {
  Budget,
  CompanyProfile,
  Desk,
  Financials,
  Group,
  IndustrialProfile,
  Member,
  Organization,
  OrganizationSetup,
} from '@/domain/organization';
import type { HttpClient } from '../http/http-client';

export interface CommodityInput {
  capacity: number;
  unit: MeasurementUnit;
  priceReference: string | null;
  currency: string;
  sells: boolean;
}

export function createOrganizationApi(http: HttpClient) {
  return {
    listMine: () => http.get<Organization[]>('/organizations'),
    create: (name: string) => http.post<Organization>('/organizations', { name }),

    // Setup da organização atual (header X-Organization-Id)
    setup: () => http.get<OrganizationSetup>('/organization'),
    rename: (name: string) => http.put<Organization>('/organization', { name }),
    updateProfile: (input: CompanyProfile) => http.put<OrganizationSetup>('/organization/profile', input),
    updateIndustrial: (input: IndustrialProfile) => http.put<OrganizationSetup>('/organization/industrial', input),
    updateBudget: (input: Budget) => http.put<OrganizationSetup>('/organization/budget', input),
    updateFinancials: (input: Omit<Financials, 'leverage'>) => http.put<OrganizationSetup>('/organization/financials', input),
    addCommodity: (input: CommodityInput & { commodity: Commodity }) =>
      http.post<OrganizationSetup>('/organization/commodities', input),
    updateCommodity: (id: string, input: CommodityInput) => http.put<OrganizationSetup>(`/organization/commodities/${id}`, input),
    removeCommodity: (id: string) => http.delete<OrganizationSetup>(`/organization/commodities/${id}`),

    /** Roles efetivas (alçadas) do usuário na organização atual. */
    roles: () => http.get<string[]>('/organization/roles'),

    members: () => http.get<Member[]>('/organization/members'),
    addMember: (input: { email: string; ruleCode: string; desk: Desk | null }) => http.post<Member[]>('/organization/members', input),
    changeMemberDesk: (memberId: string, desk: Desk | null) =>
      http.patch<Member[]>(`/organization/members/${memberId}/desk`, { desk }),
    removeMember: (memberId: string) => http.delete(`/organization/members/${memberId}`),
    groups: () => http.get<Group[]>('/organization/groups'),
    createGroup: (input: { name: string; ruleCodes: string[] }) => http.post<Group[]>('/organization/groups', input),
    addGroupMember: (groupId: string, memberId: string) =>
      http.post<Group[]>(`/organization/groups/${groupId}/members`, { memberId }),
  };
}
