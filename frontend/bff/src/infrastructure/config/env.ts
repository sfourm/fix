import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { z } from 'zod';

const DEV_SECRET = 'dev-only-session-secret-change-me-0123456789';
const repoProtos = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../../../../protos');

const schema = z.object({
  NODE_ENV: z.enum(['development', 'production', 'test']).default('development'),
  PORT: z.coerce.number().int().positive().default(3000),
  CORE_GRPC_URL: z.string().min(1).default('localhost:5098'),
  CORE_GRPC_TLS: z.stringbool().default(false),
  CORE_GRPC_DEADLINE_MS: z.coerce.number().int().positive().default(10_000),
  /** storage-service (uploads): o envio do arquivo tem prazo maior. */
  STORAGE_GRPC_URL: z.string().min(1).default('localhost:5099'),
  STORAGE_GRPC_DEADLINE_MS: z.coerce.number().int().positive().default(60_000),
  /** RabbitMQ: progresso do processamento dos arquivos (file.progress), repassado ao web por SSE. */
  RABBITMQ_URL: z.string().default('amqp://fix:fix@localhost:5672'),
  RABBITMQ_FILES_EXCHANGE: z.string().default('fix.files'),
  GRPC_CONTRACTS: z.enum(['files', 'reflection']).default('files'),
  PROTOS_DIR: z.string().default(repoProtos),
  SESSION_SECRET: z.string().min(32).default(DEV_SECRET),
  SESSION_TTL: z.string().default('8h'),
  CORS_ORIGIN: z.string().default('http://localhost:5173'),
  ELASTICSEARCH_URL: z.url().default('http://localhost:9200'),
  ELASTICSEARCH_INDEX_PREFIX: z.string().regex(/^[a-z0-9-]+$/).default('fix'),
  REDIS_URL: z.string().default('redis://localhost:6379'),
  /** TTL do cache de visualização (linhas lidas do core para pesquisas e widgets). */
  VIZ_CACHE_TTL_SECONDS: z.coerce.number().int().min(0).default(30),
  /** Ativação e endpoint do OpenTelemetry. */
  OTEL_ENABLED: z.stringbool().default(true),
  OTEL_EXPORTER_OTLP_ENDPOINT: z.string().default('http://localhost:4318'),
  OTEL_SERVICE_NAME: z.string().default('fix-bff'),
});

export type Env = z.infer<typeof schema>;

export function loadEnv(source: NodeJS.ProcessEnv = process.env): Env {
  const env = schema.parse(source);

  if (env.NODE_ENV === 'production' && env.SESSION_SECRET === DEV_SECRET) {
    throw new Error('SESSION_SECRET precisa ser configurado em produção.');
  }

  return { ...env, PROTOS_DIR: path.resolve(env.PROTOS_DIR) };
}
