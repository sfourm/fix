import type { RuleDto } from '../../../application/rules/dtos/rule.dto.js';
import type { RuleGateway } from '../../../application/rules/ports/rule.gateway.js';
import type { CoreClient } from '../core-client.js';

export class GrpcRuleGateway implements RuleGateway {
  constructor(private readonly core: CoreClient) {}

  async list(): Promise<RuleDto[]> {
    const response = await this.core.call<{ rules: RuleDto[] }>('RuleService', 'ListRules', {});
    return response.rules.map((r) => ({ id: r.id, code: r.code, name: r.name, roles: r.roles }));
  }
}
