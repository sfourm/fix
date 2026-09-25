import { AuthService } from './application/auth/services/auth.service.js';
import { CounterpartyService } from './application/counterparties/services/counterparty.service.js';
import { MandateService } from './application/mandates/services/mandate.service.js';
import { OrderService } from './application/orders/services/order.service.js';
import { OrganizationService } from './application/organizations/services/organization.service.js';
import { PolicyService } from './application/policies/services/policy.service.js';
import { RoleService } from './application/roles/services/role.service.js';
import { RuleService } from './application/rules/services/rule.service.js';
import { TimelineService } from './application/timeline/services/timeline.service.js';
import { loadEnv } from './infrastructure/config/env.js';
import { CoreClient } from './infrastructure/grpc/core-client.js';
import { createGrpcGateways } from './infrastructure/grpc/gateways/index.js';
import { JwtSessionTokenService } from './infrastructure/security/jwt-session-token.service.js';
import { createApp } from './presentation/http/app.js';

// Composition root: liga as implementações de infraestrutura às portas da aplicação.
const env = loadEnv();
const core = await CoreClient.connect(env);
const gateways = createGrpcGateways(core);
const tokens = new JwtSessionTokenService(env.SESSION_SECRET, env.SESSION_TTL);

const app = createApp({
  corsOrigin: env.CORS_ORIGIN,
  tokens,
  services: {
    auth: new AuthService(gateways.auth, tokens),
    organizations: new OrganizationService(gateways.organizations),
    counterparties: new CounterpartyService(gateways.counterparties),
    policies: new PolicyService(gateways.policies),
    mandates: new MandateService(gateways.mandates),
    orders: new OrderService(gateways.orders),
    roles: new RoleService(gateways.roles),
    rules: new RuleService(gateways.rules),
    timeline: new TimelineService(gateways.timeline),
  },
});

const server = app.listen(env.PORT, () => {
  console.log(`BFF ouvindo em http://localhost:${env.PORT} (core gRPC: ${env.CORE_GRPC_URL}, contratos: ${env.GRPC_CONTRACTS})`);
});

const shutdown = () => {
  server.close(() => {
    core.close();
    process.exit(0);
  });
};

process.on('SIGINT', shutdown);
process.on('SIGTERM', shutdown);
