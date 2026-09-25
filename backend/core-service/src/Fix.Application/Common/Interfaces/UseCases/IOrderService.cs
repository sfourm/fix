using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Dtos;
using Fix.Application.Orders.Queries;
using Fix.Domain.Abstractions;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Boletas de hedge: registro, aprovação (consome saldo do mandato) e confirmation. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IOrderService
{
    Task<OrderDto> RegisterOrderAsync(RegisterOrderCommand command, CancellationToken cancellationToken);

    Task<OrderDto> UpdateOrderAsync(UpdateOrderCommand command, CancellationToken cancellationToken);

    Task<OrderDto> ApproveOrderAsync(ApproveOrderCommand command, CancellationToken cancellationToken);

    Task<OrderDto> RejectOrderAsync(RejectOrderCommand command, CancellationToken cancellationToken);

    Task DeleteOrderAsync(DeleteOrderCommand command, CancellationToken cancellationToken);

    Task<OrderDto> ConfirmOrderAsync(ConfirmOrderCommand command, CancellationToken cancellationToken);

    Task<OrderDto> MarkOrderDivergentAsync(MarkOrderDivergentCommand command, CancellationToken cancellationToken);

    Task<OrderDto> RefuseOrderConfirmationAsync(RefuseOrderConfirmationCommand command, CancellationToken cancellationToken);

    Task<OrderDto> ResolveOrderDivergenceAsync(ResolveOrderDivergenceCommand command, CancellationToken cancellationToken);

    Task<OrderDto> GetOrderAsync(GetOrderQuery query, CancellationToken cancellationToken);

    Task<PagedList<OrderDto>> ListOrdersAsync(ListOrdersQuery query, CancellationToken cancellationToken);
}
