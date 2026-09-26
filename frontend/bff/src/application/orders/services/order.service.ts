import { mapPage, type Page } from '../../../cross-cutting/paging/page.js';
import type { LinkOrderMandateCommand } from '../commands/link-order-mandate.command.js';
import type { ApproveOrderCommand } from '../commands/approve-order.command.js';
import type { ConfirmOrderCommand } from '../commands/confirm-order.command.js';
import type { DeleteOrderCommand } from '../commands/delete-order.command.js';
import type { MarkOrderDivergentCommand } from '../commands/mark-order-divergent.command.js';
import type { RefuseOrderConfirmationCommand } from '../commands/refuse-order-confirmation.command.js';
import type { RegisterOrderCommand } from '../commands/register-order.command.js';
import type { RejectOrderCommand } from '../commands/reject-order.command.js';
import type { ResolveOrderDivergenceCommand } from '../commands/resolve-order-divergence.command.js';
import type { UpdateOrderCommand } from '../commands/update-order.command.js';
import type { OrderDto } from '../dtos/order.dto.js';
import { toOrderResponse } from '../mappers/order.mapper.js';
import type { OrderGateway } from '../ports/order.gateway.js';
import type { GetOrderQuery } from '../queries/get-order.query.js';
import type { ListOrdersQuery } from '../queries/list-orders.query.js';
import type { OrderResponse } from '../responses/order.response.js';

/** Boletas de hedge: aprovação (consome saldo do mandato) e confirmação do middle office. */
export class OrderService {
  constructor(private readonly gateway: OrderGateway) {}

  register = (command: RegisterOrderCommand) => this.respond(this.gateway.register(command));
  update = (command: UpdateOrderCommand) => this.respond(this.gateway.update(command));
  link = (command: LinkOrderMandateCommand) => this.respond(this.gateway.link(command));
  approve = (command: ApproveOrderCommand) => this.respond(this.gateway.approve(command));
  reject = (command: RejectOrderCommand) => this.respond(this.gateway.reject(command));
  confirm = (command: ConfirmOrderCommand) => this.respond(this.gateway.confirm(command));
  markDivergent = (command: MarkOrderDivergentCommand) => this.respond(this.gateway.markDivergent(command));
  refuseConfirmation = (command: RefuseOrderConfirmationCommand) => this.respond(this.gateway.refuseConfirmation(command));
  resolveDivergence = (command: ResolveOrderDivergenceCommand) => this.respond(this.gateway.resolveDivergence(command));
  get = (query: GetOrderQuery) => this.respond(this.gateway.get(query));

  async delete(command: DeleteOrderCommand): Promise<void> {
    await this.gateway.delete(command);
  }

  async list(query: ListOrdersQuery): Promise<Page<OrderResponse>> {
    return mapPage(await this.gateway.list(query), toOrderResponse);
  }

  private async respond(order: Promise<OrderDto>): Promise<OrderResponse> {
    return toOrderResponse(await order);
  }
}
