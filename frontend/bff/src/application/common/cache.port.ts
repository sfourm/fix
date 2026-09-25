/**
 * Cache de visualização (Redis na infraestrutura). Falhas de cache nunca derrubam a requisição:
 * a implementação devolve "miss" e segue sem cachear.
 */
export interface CachePort {
  get<T>(key: string): Promise<T | null>;
  set(key: string, value: unknown, ttlSeconds: number): Promise<void>;
  /** Remove todas as chaves que começam com o prefixo. */
  invalidate(prefix: string): Promise<void>;
}
