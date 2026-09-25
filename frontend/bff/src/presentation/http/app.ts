import cors from 'cors';
import express, { type Express } from 'express';
import type { SessionTokenPort } from '../../application/auth/ports/session-token.port.js';
import type { AuthService } from '../../application/auth/services/auth.service.js';
import type { VisualizationCache } from '../../application/common/visualization-cache.js';
import type { CounterpartyService } from '../../application/counterparties/services/counterparty.service.js';
import type { DashboardService } from '../../application/dashboards/services/dashboard.service.js';
import type { SavedFilterService } from '../../application/filters/services/saved-filter.service.js';
import type { MandateService } from '../../application/mandates/services/mandate.service.js';
import type { OrderService } from '../../application/orders/services/order.service.js';
import type { OrganizationService } from '../../application/organizations/services/organization.service.js';
import type { PolicyService } from '../../application/policies/services/policy.service.js';
import type { RoleService } from '../../application/roles/services/role.service.js';
import type { RuleService } from '../../application/rules/services/rule.service.js';
import type { SearchService } from '../../application/search/services/search.service.js';
import type { TimelineService } from '../../application/timeline/services/timeline.service.js';
import { authenticate, errorHandler, invalidateVisualizationOnWrite, notFound, ORGANIZATION_HEADER, resolveOrganization } from './middlewares.js';
import { authRoutes } from './routes/auth.routes.js';
import { dashboardRoutes } from './routes/dashboard.routes.js';
import { counterpartyRoutes } from './routes/counterparty.routes.js';
import { mandateRoutes } from './routes/mandate.routes.js';
import { orderRoutes } from './routes/order.routes.js';
import { currentOrganizationRoutes, organizationsRoutes } from './routes/organization.routes.js';
import { policyRoutes } from './routes/policy.routes.js';
import { roleRoutes } from './routes/role.routes.js';
import { ruleRoutes } from './routes/rule.routes.js';
import { savedFilterRoutes } from './routes/saved-filter.routes.js';
import { searchRoutes } from './routes/search.routes.js';
import { timelineRoutes } from './routes/timeline.routes.js';

export interface AppDependencies {
  corsOrigin: string;
  tokens: SessionTokenPort;
  visualizationCache: VisualizationCache;
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
    dashboards: DashboardService;
    savedFilters: SavedFilterService;
    search: SearchService;
  };
}

export function createApp({ corsOrigin, tokens, visualizationCache, services }: AppDependencies): Express {
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
  const invalidateOnWrite = invalidateVisualizationOnWrite(visualizationCache);
  app.use('/api/organizations', organizationsRoutes(services.organizations));
  app.use('/api/organization', invalidateOnWrite, currentOrganizationRoutes(services.organizations));
  app.use('/api/counterparties', invalidateOnWrite, counterpartyRoutes(services.counterparties));
  app.use('/api/policies', invalidateOnWrite, policyRoutes(services.policies));
  app.use('/api/mandates', invalidateOnWrite, mandateRoutes(services.mandates));
  app.use('/api/orders', invalidateOnWrite, orderRoutes(services.orders));
  app.use('/api/roles', roleRoutes(services.roles));
  app.use('/api/rules', invalidateOnWrite, ruleRoutes(services.rules));
  app.use('/api/timeline', timelineRoutes(services.timeline));

  // Recursos próprios do BFF (Elasticsearch + cache Redis): não passam pelo core.
  app.use('/api/dashboards', dashboardRoutes(services.dashboards));
  app.use('/api/filters', savedFilterRoutes(services.savedFilters));
  app.use('/api/search', searchRoutes(services.search));

  app.use(notFound);
  app.use(errorHandler);

  return app;
}
