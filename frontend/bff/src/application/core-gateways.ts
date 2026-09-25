import type { AuthGateway } from './auth/ports/auth.gateway.js';
import type { CounterpartyGateway } from './counterparties/ports/counterparty.gateway.js';
import type { MandateGateway } from './mandates/ports/mandate.gateway.js';
import type { OrderGateway } from './orders/ports/order.gateway.js';
import type { OrganizationGateway } from './organizations/ports/organization.gateway.js';
import type { PolicyGateway } from './policies/ports/policy.gateway.js';
import type { RoleGateway } from './roles/ports/role.gateway.js';
import type { RuleGateway } from './rules/ports/rule.gateway.js';
import type { TimelineGateway } from './timeline/ports/timeline.gateway.js';

/** Conjunto de portas para o core .NET, entregue pelo composition root. */
export interface CoreGateways {
  auth: AuthGateway;
  organizations: OrganizationGateway;
  counterparties: CounterpartyGateway;
  policies: PolicyGateway;
  mandates: MandateGateway;
  orders: OrderGateway;
  roles: RoleGateway;
  rules: RuleGateway;
  timeline: TimelineGateway;
}
