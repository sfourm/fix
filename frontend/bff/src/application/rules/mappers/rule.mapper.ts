import type { RuleDto } from '../dtos/rule.dto.js';
import type { RuleResponse } from '../responses/rule.response.js';

export const toRuleResponse = (dto: RuleDto): RuleResponse => ({ ...dto, roles: [...dto.roles] });
