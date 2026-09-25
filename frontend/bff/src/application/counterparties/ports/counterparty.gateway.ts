import type { CreateCounterpartyCommand } from '../commands/create-counterparty.command.js';
import type { DeleteCounterpartyCommand } from '../commands/delete-counterparty.command.js';
import type { SetCounterpartyHomologationCommand } from '../commands/set-counterparty-homologation.command.js';
import type { UpdateCounterpartyCommand } from '../commands/update-counterparty.command.js';
import type { CounterpartyDto } from '../dtos/counterparty.dto.js';
import type { GetCounterpartyQuery } from '../queries/get-counterparty.query.js';
import type { ListCounterpartiesQuery } from '../queries/list-counterparties.query.js';

export interface CounterpartyGateway {
  create(command: CreateCounterpartyCommand): Promise<CounterpartyDto>;
  update(command: UpdateCounterpartyCommand): Promise<CounterpartyDto>;
  setHomologation(command: SetCounterpartyHomologationCommand): Promise<CounterpartyDto>;
  delete(command: DeleteCounterpartyCommand): Promise<void>;
  get(query: GetCounterpartyQuery): Promise<CounterpartyDto>;
  list(query: ListCounterpartiesQuery): Promise<CounterpartyDto[]>;
}
