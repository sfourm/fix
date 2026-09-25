import type { CreateRuleCommand } from '../commands/create-rule.command.js';
import type { DeleteRuleCommand } from '../commands/delete-rule.command.js';
import type { UpdateRuleCommand } from '../commands/update-rule.command.js';
import type { RuleDto } from '../dtos/rule.dto.js';
import type { ListRulesQuery } from '../queries/list-rules.query.js';

export interface RuleGateway {
  list(query: ListRulesQuery): Promise<RuleDto[]>;
  create(command: CreateRuleCommand): Promise<RuleDto>;
  update(command: UpdateRuleCommand): Promise<RuleDto>;
  delete(command: DeleteRuleCommand): Promise<void>;
}
