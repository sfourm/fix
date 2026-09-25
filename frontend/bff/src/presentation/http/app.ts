import cors from 'cors';
import express, { type Express } from 'express';
import type { SessionTokenPort } from '../../application/auth/ports/session-token.port.js';
import type { AuthService } from '../../application/auth/services/auth.service.js';
import type { CounterpartyService } from '../../application/counterparties/services/counterparty.service.js';
import type { MandateService } from '../../application/mandates/services/mandate.service.js';
import type { OrderService } from '../../application/orders/services/order.service.js';
import type { OrganizationService } from '../../application/organizations/services/organization.service.js';
import type { PolicyService } from '../../application/policies/services/policy.service.js';
import type { RoleService } from '../../application/roles/services/role.service.js';
import type { RuleService } from '../../application/rules/services/rule.service.js';
import type { TimelineService } from '../../application/timeline/services/timeline.service.js';
import { authenticate, errorHandler, notFound, ORGANIZATION_HEADER, resolveOrganization } from './middlewares.js';
import { authRoutes } from './routes/auth.routes.js';
import { counterpartyRoutes } from './routes/counterparty.routes.js';
import { mandateRoutes } from './routes/mandate.routes.js';
import { orderRoutes } from './routes/order.routes.js';
import { currentOrganizationRoutes, organizationsRoutes } from './routes/organization.routes.js';
import { policyRoutes } from './routes/policy.routes.js';
import { roleRoutes } from './routes/role.routes.js';
import { ruleRoutes } from './routes/rule.routes.js';
import { timelineRoutes } from './routes/timeline.routes.js';

export interface AppDependencies {
  corsOrigin: string;
  tokens: SessionTokenPort;
  services: {
    auth: AuthService;
    organizations: OrganizationService;
    counterparties: CounterpartyService;
    policies: PolicyService;
    mandates: MandateService;
    orders: OrderService;
    roles: RoleService;
    rules: RuleService;
    timeline: TimelineService;
  };
}

export function createApp({ corsOrigin, tokens, services }: AppDependencies): Express {
  const app = express();
  const requireSession = authenticate(tokens);

  app.disable('x-powered-by');
  app.use(
    cors({
      origin: corsOrigin.split(',').map((origin) => origin.trim()),
      allowedHeaders: ['Content-Type', 'Authorization', ORGANIZATION_HEADER],
    }),
  );
  app.use(express.json({ limit: '1mb' }));

  app.get('/health', (_req, res) => {
    res.json({ status: 'ok' });
  });

  app.use('/api/auth', authRoutes(services.auth, requireSession));

  // A partir daqui toda rota exige sessão; o tenant vem do header X-Organization-Id.
  app.use('/api', requireSession, resolveOrganization);
  app.use('/api/organizations', organizationsRoutes(services.organizations));
  app.use('/api/organization', currentOrganizationRoutes(services.organizations));
  app.use('/api/counterparties', counterpartyRoutes(services.counterparties));
  app.use('/api/policies', policyRoutes(services.policies));
  app.use('/api/mandates', mandateRoutes(services.mandates));
  app.use('/api/orders', orderRoutes(services.orders));
  app.use('/api/roles', roleRoutes(services.roles));
  app.use('/api/rules', ruleRoutes(services.rules));
  app.use('/api/timeline', timelineRoutes(services.timeline));

  app.use(notFound);
  app.use(errorHandler);

  return app;
}
