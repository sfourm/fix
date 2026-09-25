import type { CreateRuleCommand } from '../../../application/rules/commands/create-rule.command.js';
import type { DeleteRuleCommand } from '../../../application/rules/commands/delete-rule.command.js';
import type { UpdateRuleCommand } from '../../../application/rules/commands/update-rule.command.js';
import type { RuleDto } from '../../../application/rules/dtos/rule.dto.js';
import type { RuleGateway } from '../../../application/rules/ports/rule.gateway.js';
import type { ListRulesQuery } from '../../../application/rules/queries/list-rules.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext } from '../mappers/common.contract-mapper.js';

const SERVICE = 'RuleService';

const toRuleDto = (r: RuleDto): RuleDto => ({
  id: r.id,
  code: r.code,
  name: r.name,
  roles: r.roles,
  isSystem: r.isSystem,
  usages: r.usages,
});

export class GrpcRuleGateway implements RuleGateway {
  constructor(private readonly core: CoreClient) {}

  async list({ context }: ListRulesQuery): Promise<RuleDto[]> {
    return (await this.call<{ rules: RuleDto[] }>('ListRules', context)).rules.map(toRuleDto);
  }

  async create({ context, name, roleCodes }: CreateRuleCommand): Promise<RuleDto> {
    return toRuleDto(await this.call<RuleDto>('CreateRule', context, { name, roleCodes }));
  }

  async update({ context, id, name, roleCodes }: UpdateRuleCommand): Promise<RuleDto> {
    return toRuleDto(await this.call<RuleDto>('UpdateRule', context, { id, name, roleCodes }));
  }

  async delete({ context, id }: DeleteRuleCommand): Promise<void> {
    await this.call('DeleteRule', context, { id });
  }

  private call<T>(method: string, context: RequestContext, request: object = {}): Promise<T> {
    return this.core.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
