import { metrics, SpanKind, SpanStatusCode, trace } from '@opentelemetry/api';
import { createClient, type RedisClientType } from 'redis';
import type { CachePort } from '../../application/common/cache.port.js';

const KEY_PREFIX = 'fix:';

// O node-redis 5+ não tem instrumentação automática: spans e métricas do cache são emitidos aqui.
const tracer = trace.getTracer('fix-bff.cache');
const lookups = metrics.getMeter('fix-bff').createCounter('cache_lookups_total', {
  description: 'Leituras do cache de visualização por resultado (hit/miss/error)',
});

/** Nome do "conjunto" da chave sem ids (`viz:{org}:rows:{user}:orders` → `viz:rows`), para span e métrica. */
function keyspace(key: string): string {
  return key
    .split(':')
    .filter((part) => part && !/^[0-9a-f-]{32,36}$/i.test(part))
    .slice(0, 2)
    .join(':');
}

async function traced<T>(operation: string, key: string, run: () => Promise<T>): Promise<T> {
  return tracer.startActiveSpan(`redis ${operation} ${keyspace(key)}`, { kind: SpanKind.CLIENT }, async (span) => {
    span.setAttributes({ 'db.system.name': 'redis', 'db.operation.name': operation, 'fix.cache.keyspace': keyspace(key) });
    try {
      return await run();
    } catch (error) {
      span.recordException(error as Error);
      span.setStatus({ code: SpanStatusCode.ERROR, message: (error as Error).message });
      throw error;
    } finally {
      span.end();
    }
  });
}

/**
 * Cache de visualização no Redis. Indisponibilidade do Redis não derruba o BFF:
 * leituras viram "miss" e escritas são ignoradas até a conexão voltar.
 */
export class RedisCache implements CachePort {
  private warned = false;

  private constructor(private readonly client: RedisClientType) {}

  static async connect(url: string): Promise<RedisCache> {
    const client = createClient({ url, socket: { reconnectStrategy: (retries) => Math.min(retries * 500, 5000) } }) as RedisClientType;
    const cache = new RedisCache(client);
    client.on('error', (error: Error) => cache.warn(error));

    try {
      await client.connect();
    } catch (error) {
      cache.warn(error as Error);
    }

    return cache;
  }

  async get<T>(key: string): Promise<T | null> {
    if (!this.client.isReady) return null;
    try {
      const raw = await traced('GET', key, () => this.client.get(KEY_PREFIX + key));
      lookups.add(1, { keyspace: keyspace(key), result: raw === null ? 'miss' : 'hit' });
      return raw === null ? null : (JSON.parse(raw) as T);
    } catch (error) {
      lookups.add(1, { keyspace: keyspace(key), result: 'error' });
      this.warn(error as Error);
      return null;
    }
  }

  async set(key: string, value: unknown, ttlSeconds: number): Promise<void> {
    if (!this.client.isReady || ttlSeconds <= 0) return;
    try {
      await traced('SET', key, () => this.client.set(KEY_PREFIX + key, JSON.stringify(value), { expiration: { type: 'EX', value: ttlSeconds } }));
    } catch (error) {
      this.warn(error as Error);
    }
  }

  async invalidate(prefix: string): Promise<void> {
    if (!this.client.isReady) return;
    try {
      await traced('INVALIDATE', prefix, async () => {
        const keys: string[] = [];
        for await (const batch of this.client.scanIterator({ MATCH: `${KEY_PREFIX}${prefix}*`, COUNT: 200 })) {
          keys.push(...batch);
        }

        if (keys.length > 0) {
          await this.client.del(keys);
        }
        trace.getActiveSpan()?.setAttribute('fix.cache.deleted_keys', keys.length);
      });
    } catch (error) {
      this.warn(error as Error);
    }
  }

  async close(): Promise<void> {
    if (this.client.isOpen) {
      await this.client.quit();
    }
  }

  private warn(error: Error): void {
    if (!this.warned) {
      console.warn(`Redis indisponível, seguindo sem cache de visualização: ${error.message}`);
      this.warned = true;
    }
  }
}
