import type { ApproveOrderCommand } from '../../../application/orders/commands/approve-order.command.js';
import type { ConfirmOrderCommand } from '../../../application/orders/commands/confirm-order.command.js';
import type { DeleteOrderCommand } from '../../../application/orders/commands/delete-order.command.js';
import type { MarkOrderDivergentCommand } from '../../../application/orders/commands/mark-order-divergent.command.js';
import type { RefuseOrderConfirmationCommand } from '../../../application/orders/commands/refuse-order-confirmation.command.js';
import type { RegisterOrderCommand } from '../../../application/orders/commands/register-order.command.js';
import type { RejectOrderCommand } from '../../../application/orders/commands/reject-order.command.js';
import type { ResolveOrderDivergenceCommand } from '../../../application/orders/commands/resolve-order-divergence.command.js';
import type { UpdateOrderCommand } from '../../../application/orders/commands/update-order.command.js';
import type { OrderDto } from '../../../application/orders/dtos/order.dto.js';
import type { OrderGateway } from '../../../application/orders/ports/order.gateway.js';
import type { GetOrderQuery } from '../../../application/orders/queries/get-order.query.js';
import type { ListOrdersQuery } from '../../../application/orders/queries/list-orders.query.js';
import type { RequestContext } from '../../../cross-cutting/context/request-context.js';
import type { Page } from '../../../cross-cutting/paging/page.js';
import type { CoreClient } from '../core-client.js';
import { toContractContext, toContractPage, toPage, type ContractPageInfo } from '../mappers/common.contract-mapper.js';
import { approvalStatusEnum, confirmationStatusEnum } from '../mappers/enum.contract-mapper.js';
import { toContractOrderTerms, toOrderDto, type ContractOrder } from '../mappers/order.contract-mapper.js';

const SERVICE = 'OrderService';

export class GrpcOrderGateway implements OrderGateway {
  constructor(private readonly core: CoreClient) {}

  register({ context, mandateId, counterpartyId, terms }: RegisterOrderCommand): Promise<OrderDto> {
    return this.order('RegisterOrder', context, { mandateId, counterpartyId, terms: toContractOrderTerms(terms) });
  }

  update({ context, id, counterpartyId, terms }: UpdateOrderCommand): Promise<OrderDto> {
    return this.order('UpdateOrder', context, { id, counterpartyId, terms: toContractOrderTerms(terms) });
  }

  approve({ context, id, note }: ApproveOrderCommand): Promise<OrderDto> {
    return this.order('ApproveOrder', context, { id, note });
  }

  reject({ context, id, note }: RejectOrderCommand): Promise<OrderDto> {
    return this.order('RejectOrder', context, { id, note });
  }

  async delete({ context, id }: DeleteOrderCommand): Promise<void> {
    await this.call('DeleteOrder', context, { id });
  }

  confirm({ context, id, receivedOn }: ConfirmOrderCommand): Promise<OrderDto> {
    return this.order('ConfirmOrder', context, { id, receivedOn });
  }

  markDivergent({ context, id, note }: MarkOrderDivergentCommand): Promise<OrderDto> {
    return this.order('MarkOrderDivergent', context, { id, note });
  }

  refuseConfirmation({ context, id, note }: RefuseOrderConfirmationCommand): Promise<OrderDto> {
    return this.order('RefuseOrderConfirmation', context, { id, note });
  }

  resolveDivergence({ context, id }: ResolveOrderDivergenceCommand): Promise<OrderDto> {
    return this.order('ResolveOrderDivergence', context, { id });
  }

  get({ context, id }: GetOrderQuery): Promise<OrderDto> {
    return this.order('GetOrder', context, { id });
  }

  async list({ context, mandateId, approval, confirmation, page }: ListOrdersQuery): Promise<Page<OrderDto>> {
    const response = await this.call<{ orders: ContractOrder[]; page: ContractPageInfo | null }>('ListOrders', context, {
      mandateId,
      approval: approvalStatusEnum.toContract(approval),
      confirmation: confirmationStatusEnum.toContract(confirmation),
      page: toContractPage(page),
    });
    return toPage(response.orders, response.page, toOrderDto);
  }

  private async order(method: string, context: RequestContext, request: object): Promise<OrderDto> {
    return toOrderDto(await this.call<ContractOrder>(method, context, request));
  }

  private call<T>(method: string, context: RequestContext, request: object): Promise<T> {
    return this.core.call<T>(SERVICE, method, { context: toContractContext(context), ...request });
  }
}
