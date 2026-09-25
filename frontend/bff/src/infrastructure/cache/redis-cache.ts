import { createClient, type RedisClientType } from 'redis';
import type { CachePort } from '../../application/common/cache.port.js';

const KEY_PREFIX = 'fix:';

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
      const raw = await this.client.get(KEY_PREFIX + key);
      return raw === null ? null : (JSON.parse(raw) as T);
    } catch (error) {
      this.warn(error as Error);
      return null;
    }
  }

  async set(key: string, value: unknown, ttlSeconds: number): Promise<void> {
    if (!this.client.isReady || ttlSeconds <= 0) return;
    try {
      await this.client.set(KEY_PREFIX + key, JSON.stringify(value), { expiration: { type: 'EX', value: ttlSeconds } });
    } catch (error) {
      this.warn(error as Error);
    }
  }

  async invalidate(prefix: string): Promise<void> {
    if (!this.client.isReady) return;
    try {
      const keys: string[] = [];
      for await (const batch of this.client.scanIterator({ MATCH: `${KEY_PREFIX}${prefix}*`, COUNT: 200 })) {
        keys.push(...batch);
      }

      if (keys.length > 0) {
        await this.client.del(keys);
      }
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
