import type { CreateRuleCommand } from '../commands/create-rule.command.js';
import type { DeleteRuleCommand } from '../commands/delete-rule.command.js';
import type { UpdateRuleCommand } from '../commands/update-rule.command.js';
import { toRuleResponse } from '../mappers/rule.mapper.js';
import type { RuleGateway } from '../ports/rule.gateway.js';
import type { ListRulesQuery } from '../queries/list-rules.query.js';
import type { RuleResponse } from '../responses/rule.response.js';

/** Rules da organização: owner e user (sistema) e as alçadas personalizadas, editáveis por ela. */
export class RuleService {
  constructor(private readonly gateway: RuleGateway) {}

  async list(query: ListRulesQuery): Promise<RuleResponse[]> {
    return (await this.gateway.list(query)).map(toRuleResponse);
  }

  async create(command: CreateRuleCommand): Promise<RuleResponse> {
    return toRuleResponse(await this.gateway.create(command));
  }

  async update(command: UpdateRuleCommand): Promise<RuleResponse> {
    return toRuleResponse(await this.gateway.update(command));
  }

  async delete(command: DeleteRuleCommand): Promise<void> {
    await this.gateway.delete(command);
  }
}
