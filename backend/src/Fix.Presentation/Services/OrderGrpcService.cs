using Fix.Application.Abstractions.Messaging;
using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Entities = Fix.Domain.AggregateRoots.Orders;

namespace Fix.Presentation.Services;

internal sealed class OrderGrpcService(IDispatcher dispatcher) : OrderService.OrderServiceBase
{
    public override async Task<Order> RegisterOrder(RegisterOrderRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RegisterOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MandateId.ToGuid("mandate_id"),
                request.CounterpartyId.ToGuid("counterparty_id"),
                request.Terms.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Order> UpdateOrder(UpdateOrderRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new UpdateOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.CounterpartyId.ToGuid("counterparty_id"),
                request.Terms.ToInput()),
            context.CancellationToken)).ToContract();

    public override async Task<Order> ApproveOrder(OrderDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new ApproveOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote)),
            context.CancellationToken)).ToContract();

    public override async Task<Order> RejectOrder(OrderDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RejectOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeleteOrder(OrderIdRequest request, ServerCallContext context)
    {
        await dispatcher.SendAsync(
            new DeleteOrderCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken);

        return new Empty();
    }

    // ---------- Confirmation ----------

    public override async Task<Order> ConfirmOrder(ConfirmOrderRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new ConfirmOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.ReceivedOn.ToDate("received_on")),
            context.CancellationToken)).ToContract();

    public override async Task<Order> MarkOrderDivergent(OrderDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new MarkOrderDivergentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            context.CancellationToken)).ToContract();

    public override async Task<Order> RefuseOrderConfirmation(OrderDecisionRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new RefuseOrderConfirmationCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            context.CancellationToken)).ToContract();

    public override async Task<Order> ResolveOrderDivergence(OrderIdRequest request, ServerCallContext context) =>
        (await dispatcher.SendAsync(
            new ResolveOrderDivergenceCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken)).ToContract();

    // ---------- Queries ----------

    public override async Task<Order> GetOrder(OrderIdRequest request, ServerCallContext context) =>
        (await dispatcher.QueryAsync(
            new GetOrderQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            context.CancellationToken)).ToContract();

    public override async Task<ListOrdersResponse> ListOrders(ListOrdersRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var orders = await dispatcher.QueryAsync(
            new ListOrdersQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MandateId.ToOptionalString(request.HasMandateId).ToOptionalGuid("mandate_id"),
                request.Approval.ToOptionalDomain<Entities.ApprovalStatus>("approval"),
                request.Confirmation.ToOptionalDomain<Entities.ConfirmationStatus>("confirmation"),
                page,
                pageSize),
            context.CancellationToken);

        return new ListOrdersResponse
        {
            Orders = { orders.Items.Select(o => o.ToContract()) },
            Page = orders.ToPageInfo(),
        };
    }
}

