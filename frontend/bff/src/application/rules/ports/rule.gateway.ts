import type { RuleDto } from '../dtos/rule.dto.js';
import type { ListRulesQuery } from '../queries/list-rules.query.js';

export interface RuleGateway {
  list(query: ListRulesQuery): Promise<RuleDto[]>;
}
