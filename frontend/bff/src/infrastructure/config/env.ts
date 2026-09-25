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
  GRPC_CONTRACTS: z.enum(['files', 'reflection']).default('files'),
  PROTOS_DIR: z.string().default(repoProtos),
  SESSION_SECRET: z.string().min(32).default(DEV_SECRET),
  SESSION_TTL: z.string().default('8h'),
  CORS_ORIGIN: z.string().default('http://localhost:5173'),
});

export type Env = z.infer<typeof schema>;

export function loadEnv(source: NodeJS.ProcessEnv = process.env): Env {
  const env = schema.parse(source);

  if (env.NODE_ENV === 'production' && env.SESSION_SECRET === DEV_SECRET) {
    throw new Error('SESSION_SECRET precisa ser configurado em produção.');
  }

  return { ...env, PROTOS_DIR: path.resolve(env.PROTOS_DIR) };
}
