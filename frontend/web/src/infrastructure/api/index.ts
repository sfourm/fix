import type { HttpClient } from '../http/http-client';
import { createAuthApi } from './auth.api';
import { createCounterpartyApi } from './counterparty.api';
import { createDashboardApi } from './dashboard.api';
import { createMandateApi } from './mandate.api';
import { createOrderApi } from './order.api';
import { createOrganizationApi } from './organization.api';
import { createPolicyApi } from './policy.api';
import { createRoleApi } from './role.api';
import { createRuleApi } from './rule.api';
import { createFilterApi, createSearchApi } from './search.api';
import { createTimelineApi } from './timeline.api';

/** Clientes REST do BFF, um por entidade. */
export function createApi(http: HttpClient) {
  return {
    auth: createAuthApi(http),
    organizations: createOrganizationApi(http),
    counterparties: createCounterpartyApi(http),
    policies: createPolicyApi(http),
    mandates: createMandateApi(http),
    orders: createOrderApi(http),
    roles: createRoleApi(http),
    rules: createRuleApi(http),
    timeline: createTimelineApi(http),
    dashboards: createDashboardApi(http),
    filters: createFilterApi(http),
    search: createSearchApi(http),
  };
}

export type Api = ReturnType<typeof createApi>;
