import type { CreateCounterpartyCommand } from '../../../application/counterparties/commands/create-counterparty.command.js';
import type { DeleteCounterpartyCommand } from '../../../application/counterparties/commands/delete-counterparty.command.js';
import type { SetCounterpartyHomologationCommand } from '../../../application/counterparties/commands/set-counterparty-homologation.command.js';
import type { UpdateCounterpartyCommand } from '../../../application/counterparties/commands/update-counterparty.command.js';
import type { CounterpartyDto } from '../../../application/counterparties/dtos/counterparty.dto.js';
import type { CounterpartyGateway } from '../../../application/counterparties/ports/counterparty.gateway.js';
import type { GetCounterpartyQuery } from '../../../application/counterparties/queries/get-counterparty.query.js';
import type { ListCounterpartiesQuery } from '../../../application/counterparties/queries/list-counterparties.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext } from '../mappers/common.contract-mapper.js';
import {
  toContractCounterpartyInput,
  toCounterpartyDto,
  type ContractCounterparty,
} from '../mappers/counterparty.contract-mapper.js';

const SERVICE = 'CounterpartyService';

export class GrpcCounterpartyGateway implements CounterpartyGateway {
  constructor(private readonly core: CoreClient) {}

  create({ context, ...data }: CreateCounterpartyCommand): Promise<CounterpartyDto> {
    return this.counterparty('CreateCounterparty', context, { data: toContractCounterpartyInput(data) });
  }

  update({ context, id, ...data }: UpdateCounterpartyCommand): Promise<CounterpartyDto> {
    return this.counterparty('UpdateCounterparty', context, { id, data: toContractCounterpartyInput(data) });
  }

  setHomologation(command: SetCounterpartyHomologationCommand): Promise<CounterpartyDto> {
    return this.counterparty('SetCounterpartyHomologation', command.context, {
      id: command.id,
      homologated: command.homologated,
    });
  }

  async delete(command: DeleteCounterpartyCommand): Promise<void> {
    await this.call('DeleteCounterparty', command.context, { id: command.id });
  }

  get(query: GetCounterpartyQuery): Promise<CounterpartyDto> {
    return this.counterparty('GetCounterparty', query.context, { id: query.id });
  }

  async list(query: ListCounterpartiesQuery): Promise<CounterpartyDto[]> {
    const response = await this.call<{ counterparties: ContractCounterparty[] }>('ListCounterparties', query.context, {
      onlyHomologated: query.onlyHomologated,
    });
    return response.counterparties.map(toCounterpartyDto);
  }

  private async counterparty(method: string, context: RequestContext, request: object): Promise<CounterpartyDto> {
    return toCounterpartyDto(await this.call<ContractCounterparty>(method, context, request));
  }

  private call<T>(method: string, context: RequestContext, request: object): Promise<T> {
    return this.core.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
