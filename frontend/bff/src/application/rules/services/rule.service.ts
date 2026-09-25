import { toRuleResponse } from '../mappers/rule.mapper.js';
import type { RuleGateway } from '../ports/rule.gateway.js';
import type { ListRulesQuery } from '../queries/list-rules.query.js';
import type { RuleResponse } from '../responses/rule.response.js';

/** Rules: conjuntos de roles atribuídos a membros e grupos. */
export class RuleService {
  constructor(private readonly gateway: RuleGateway) {}

  async list(query: ListRulesQuery): Promise<RuleResponse[]> {
    return (await this.gateway.list(query)).map(toRuleResponse);
  }
}
