import { AuthService } from './application/auth/services/auth.service.js';
import { ActorResolver } from './application/common/actor-resolver.js';
import { VisualizationCache } from './application/common/visualization-cache.js';
import { CounterpartyService } from './application/counterparties/services/counterparty.service.js';
import { DashboardService } from './application/dashboards/services/dashboard.service.js';
import { SavedFilterService } from './application/filters/services/saved-filter.service.js';
import { MandateService } from './application/mandates/services/mandate.service.js';
import { OrderService } from './application/orders/services/order.service.js';
import { OrganizationService } from './application/organizations/services/organization.service.js';
import { PolicyService } from './application/policies/services/policy.service.js';
import { RoleService } from './application/roles/services/role.service.js';
import { RuleService } from './application/rules/services/rule.service.js';
import { SourceRowReader } from './application/search/engine/source-row-reader.js';
import { SearchService } from './application/search/services/search.service.js';
import { TimelineService } from './application/timeline/services/timeline.service.js';
import { RedisCache } from './infrastructure/cache/redis-cache.js';
import { loadEnv } from './infrastructure/config/env.js';
import { ElasticDashboardRepository } from './infrastructure/elasticsearch/elastic-dashboard.repository.js';
import { ElasticSavedFilterRepository } from './infrastructure/elasticsearch/elastic-saved-filter.repository.js';
import { connectElasticsearch } from './infrastructure/elasticsearch/elasticsearch-connection.js';
import { CoreClient } from './infrastructure/grpc/core-client.js';
import { createGrpcGateways } from './infrastructure/grpc/gateways/index.js';
import { JwtSessionTokenService } from './infrastructure/security/jwt-session-token.service.js';
import { createApp } from './presentation/http/app.js';

// Composition root: liga as implementações de infraestrutura às portas da aplicação e aos repositórios do domínio.
const env = loadEnv();
const core = await CoreClient.connect(env);
const gateways = createGrpcGateways(core);
const tokens = new JwtSessionTokenService(env.SESSION_SECRET, env.SESSION_TTL);

// Recursos próprios do BFF: dashboards e filtros no Elasticsearch, cache de visualização no Redis.
const elastic = await connectElasticsearch(env.ELASTICSEARCH_URL, env.ELASTICSEARCH_INDEX_PREFIX);
const dashboardRepository = new ElasticDashboardRepository(elastic.client, elastic.indices.dashboards);
const savedFilterRepository = new ElasticSavedFilterRepository(elastic.client, elastic.indices.savedFilters);
const redis = await RedisCache.connect(env.REDIS_URL);
const visualizationCache = new VisualizationCache(redis, env.VIZ_CACHE_TTL_SECONDS);

const organizations = new OrganizationService(gateways.organizations);
const counterparties = new CounterpartyService(gateways.counterparties);
const policies = new PolicyService(gateways.policies);
const mandates = new MandateService(gateways.mandates);
const orders = new OrderService(gateways.orders);

const actors = new ActorResolver(gateways.organizations, visualizationCache);
const rowReader = new SourceRowReader({ orders, mandates, counterparties, policies, cache: visualizationCache });
const search = new SearchService(rowReader, savedFilterRepository, actors);

const app = createApp({
  corsOrigin: env.CORS_ORIGIN,
  tokens,
  visualizationCache,
  services: {
    auth: new AuthService(gateways.auth, tokens),
    organizations,
    counterparties,
    policies,
    mandates,
    orders,
    roles: new RoleService(gateways.roles),
    rules: new RuleService(gateways.rules),
    timeline: new TimelineService(gateways.timeline),
    dashboards: new DashboardService(dashboardRepository, savedFilterRepository, search, actors),
    savedFilters: new SavedFilterService(savedFilterRepository, dashboardRepository, actors),
    search,
  },
});

const server = app.listen(env.PORT, () => {
  console.log(
    `BFF ouvindo em http://localhost:${env.PORT} (core gRPC: ${env.CORE_GRPC_URL}, contratos: ${env.GRPC_CONTRACTS}, ` +
      `elasticsearch: ${env.ELASTICSEARCH_URL}, redis: ${env.REDIS_URL})`,
  );
});

const shutdown = () => {
  server.close(async () => {
    core.close();
    await Promise.allSettled([redis.close(), elastic.client.close()]);
    process.exit(0);
  });
};

process.on('SIGINT', shutdown);
process.on('SIGTERM', shutdown);
