import type { estypes } from '@elastic/elasticsearch';

type QueryDslQueryContainer = estypes.QueryDslQueryContainer;

/** Recursos da organização visíveis ao usuário: os próprios (qualquer visibilidade) e os públicos. */
export const visibleTo = (organizationId: string, userId: string, extra: QueryDslQueryContainer[] = []): QueryDslQueryContainer => ({
  bool: {
    filter: [{ term: { organizationId } }, ...extra],
    should: [{ term: { ownerId: userId } }, { term: { visibility: 'Public' } }],
    minimum_should_match: 1,
  },
});

export const MAX_RESULTS = 500;
