import { Client } from '@elastic/elasticsearch';
import type { estypes } from '@elastic/elasticsearch';

type MappingTypeMapping = estypes.MappingTypeMapping;

/** Índices do BFF. Os documentos guardam o estado das entidades do domínio (dashboards e filtros salvos). */
export interface BffIndices {
  dashboards: string;
  savedFilters: string;
}

const keyword = { type: 'keyword' } as const;
const date = { type: 'date' } as const;
const name = { type: 'text', fields: { sort: { type: 'keyword', normalizer: 'lowercase' } } } as const;

// dynamic: false — só os campos de busca são indexados; widgets e critérios ficam só no _source.
const mappings: Record<keyof BffIndices, MappingTypeMapping> = {
  dashboards: {
    dynamic: false,
    properties: {
      organizationId: keyword,
      ownerId: keyword,
      visibility: keyword,
      name,
      filterIds: keyword,
      createdAt: date,
      updatedAt: date,
    },
  },
  savedFilters: {
    dynamic: false,
    properties: {
      organizationId: keyword,
      ownerId: keyword,
      visibility: keyword,
      source: keyword,
      name,
      createdAt: date,
      updatedAt: date,
    },
  },
};

export async function connectElasticsearch(url: string, prefix: string): Promise<{ client: Client; indices: BffIndices }> {
  const client = new Client({ node: url });
  const indices: BffIndices = { dashboards: `${prefix}-dashboards`, savedFilters: `${prefix}-saved-filters` };

  const settings = { analysis: { normalizer: { lowercase: { type: 'custom' as const, filter: ['lowercase', 'asciifolding'] } } } };
  for (const key of Object.keys(indices) as (keyof BffIndices)[]) {
    const index = indices[key];
    if (!(await client.indices.exists({ index }))) {
      await client.indices.create({ index, settings, mappings: mappings[key] });
    }
  }

  return { client, indices };
}
