import type { CachePort } from './cache.port.js';

/**
 * Chaves do cache de visualização, agrupadas por organização ("viz:{org}:"): qualquer alteração de dados
 * da organização feita pelo BFF invalida o grupo inteiro; alterações de outros canais expiram pelo TTL.
 */
export class VisualizationCache {
  constructor(
    private readonly cache: CachePort,
    private readonly ttlSeconds: number,
  ) {}

  async remember<T>(organizationId: string, key: string, load: () => Promise<T>, ttlSeconds = this.ttlSeconds): Promise<{ value: T; cached: boolean }> {
    const fullKey = `${VisualizationCache.prefix(organizationId)}${key}`;
    const hit = await this.cache.get<T>(fullKey);
    if (hit !== null) {
      return { value: hit, cached: true };
    }

    const value = await load();
    await this.cache.set(fullKey, value, ttlSeconds);
    return { value, cached: false };
  }

  invalidateOrganization(organizationId: string): Promise<void> {
    return this.cache.invalidate(VisualizationCache.prefix(organizationId));
  }

  private static prefix(organizationId: string): string {
    return `viz:${organizationId}:`;
  }
}
