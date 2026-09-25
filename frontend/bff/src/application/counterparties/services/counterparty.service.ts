import type { CreateCounterpartyCommand } from '../commands/create-counterparty.command.js';
import type { DeleteCounterpartyCommand } from '../commands/delete-counterparty.command.js';
import type { SetCounterpartyHomologationCommand } from '../commands/set-counterparty-homologation.command.js';
import type { UpdateCounterpartyCommand } from '../commands/update-counterparty.command.js';
import { toCounterpartyResponse } from '../mappers/counterparty.mapper.js';
import type { CounterpartyGateway } from '../ports/counterparty.gateway.js';
import type { GetCounterpartyQuery } from '../queries/get-counterparty.query.js';
import type { ListCounterpartiesQuery } from '../queries/list-counterparties.query.js';
import type { CounterpartyResponse } from '../responses/counterparty.response.js';

export class CounterpartyService {
  constructor(private readonly gateway: CounterpartyGateway) {}

  async create(command: CreateCounterpartyCommand): Promise<CounterpartyResponse> {
    return toCounterpartyResponse(await this.gateway.create(command));
  }

  async update(command: UpdateCounterpartyCommand): Promise<CounterpartyResponse> {
    return toCounterpartyResponse(await this.gateway.update(command));
  }

  async setHomologation(command: SetCounterpartyHomologationCommand): Promise<CounterpartyResponse> {
    return toCounterpartyResponse(await this.gateway.setHomologation(command));
  }

  async delete(command: DeleteCounterpartyCommand): Promise<void> {
    await this.gateway.delete(command);
  }

  async get(query: GetCounterpartyQuery): Promise<CounterpartyResponse> {
    return toCounterpartyResponse(await this.gateway.get(query));
  }

  async list(query: ListCounterpartiesQuery): Promise<CounterpartyResponse[]> {
    return (await this.gateway.list(query)).map(toCounterpartyResponse);
  }
}
