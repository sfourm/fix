import type { CreateCounterpartyCommand } from '../../../application/counterparties/commands/create-counterparty.command.js';
import type { CounterpartyDto } from '../../../application/counterparties/dtos/counterparty.dto.js';
import { nullable, nullableNumber } from './common.contract-mapper.js';
import { counterpartyTypeEnum } from './enum.contract-mapper.js';

export interface ContractCounterparty {
  id: string;
  name: string;
  type: string;
  document?: string;
  address?: string;
  country?: string;
  isHomologated: boolean;
  notionalLimitUsd?: number;
  mtmLimitUsd?: number;
}

export const toCounterpartyDto = (c: ContractCounterparty): CounterpartyDto => ({
  id: c.id,
  name: c.name,
  type: counterpartyTypeEnum.fromContractRequired(c.type),
  document: nullable(c.document),
  address: nullable(c.address),
  country: nullable(c.country),
  isHomologated: c.isHomologated,
  notionalLimitUsd: nullableNumber(c.notionalLimitUsd),
  mtmLimitUsd: nullableNumber(c.mtmLimitUsd),
});

export const toContractCounterpartyInput = (c: Omit<CreateCounterpartyCommand, 'context'>) => ({
  name: c.name,
  type: counterpartyTypeEnum.toContract(c.type),
  document: c.document,
  address: c.address,
  country: c.country,
  notionalLimitUsd: c.notionalLimitUsd,
  mtmLimitUsd: c.mtmLimitUsd,
});
