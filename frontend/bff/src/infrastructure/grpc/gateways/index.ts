import type { CoreGateways } from '../../../application/core-gateways.js';
import type { CoreClient } from '../core-client.js';
import { GrpcAuthGateway } from './auth.gateway.js';
import { GrpcCounterpartyGateway } from './counterparty.gateway.js';
import { GrpcMandateGateway } from './mandate.gateway.js';
import { GrpcOrderGateway } from './order.gateway.js';
import { GrpcOrganizationGateway } from './organization.gateway.js';
import { GrpcPolicyGateway } from './policy.gateway.js';
import { GrpcRoleGateway } from './role.gateway.js';
import { GrpcRuleGateway } from './rule.gateway.js';
import { GrpcTimelineGateway } from './timeline.gateway.js';

export function createGrpcGateways(core: CoreClient): CoreGateways {
  return {
    auth: new GrpcAuthGateway(core),
    organizations: new GrpcOrganizationGateway(core),
    counterparties: new GrpcCounterpartyGateway(core),
    policies: new GrpcPolicyGateway(core),
    mandates: new GrpcMandateGateway(core),
    orders: new GrpcOrderGateway(core),
    roles: new GrpcRoleGateway(core),
    rules: new GrpcRuleGateway(core),
    timeline: new GrpcTimelineGateway(core),
  };
}
