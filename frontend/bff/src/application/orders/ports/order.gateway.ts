import type { Page } from '../../../cross-cutting/paging/page.js';
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
import type { GetOrderQuery } from '../queries/get-order.query.js';
import type { ListOrdersQuery } from '../queries/list-orders.query.js';

export interface OrderGateway {
  register(command: RegisterOrderCommand): Promise<OrderDto>;
  update(command: UpdateOrderCommand): Promise<OrderDto>;
  link(command: LinkOrderMandateCommand): Promise<OrderDto>;
  approve(command: ApproveOrderCommand): Promise<OrderDto>;
  reject(command: RejectOrderCommand): Promise<OrderDto>;
  delete(command: DeleteOrderCommand): Promise<void>;
  confirm(command: ConfirmOrderCommand): Promise<OrderDto>;
  markDivergent(command: MarkOrderDivergentCommand): Promise<OrderDto>;
  refuseConfirmation(command: RefuseOrderConfirmationCommand): Promise<OrderDto>;
  resolveDivergence(command: ResolveOrderDivergenceCommand): Promise<OrderDto>;
  get(query: GetOrderQuery): Promise<OrderDto>;
  list(query: ListOrdersQuery): Promise<Page<OrderDto>>;
}
