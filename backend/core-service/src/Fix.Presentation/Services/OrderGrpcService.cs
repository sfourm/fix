using Fix.Application.Abstractions.Validation;
using Fix.Application.Common.Interfaces.UseCases;
using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Queries;
using Fix.Contracts.V1;
using Fix.Presentation.Mappers;
using Fix.Presentation.UseCases;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Entities = Fix.Domain.AggregateRoots.Orders;

namespace Fix.Presentation.Services;

internal sealed class OrderGrpcService(
    IValidationFactory validation,
    IOrderService orderService) : OrderService.OrderServiceBase
{
    public override async Task<Order> RegisterOrder(RegisterOrderRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RegisterOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MandateId.ToOptionalString(request.HasMandateId).ToOptionalGuid("mandate_id"),
                request.CounterpartyId.ToGuid("counterparty_id"),
                request.Terms.ToInput()),
            orderService.RegisterOrderAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> UpdateOrder(UpdateOrderRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new UpdateOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.CounterpartyId.ToGuid("counterparty_id"),
                request.Terms.ToInput()),
            orderService.UpdateOrderAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> LinkOrderMandate(LinkOrderMandateRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new LinkOrderMandateCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.MandateId.ToGuid("mandate_id"),
                request.Justification),
            orderService.LinkOrderMandateAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> ApproveOrder(OrderDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new ApproveOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote)),
            orderService.ApproveOrderAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> RejectOrder(OrderDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RejectOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            orderService.RejectOrderAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Empty> DeleteOrder(OrderIdRequest request, ServerCallContext context)
    {
        await validation.ExecuteAsync(
            new DeleteOrderCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            orderService.DeleteOrderAsync,
            context.CancellationToken);

        return new Empty();
    }

    // ---------- Confirmation ----------

    public override async Task<Order> ConfirmOrder(ConfirmOrderRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new ConfirmOrderCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.ReceivedOn.ToDate("received_on")),
            orderService.ConfirmOrderAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> MarkOrderDivergent(OrderDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new MarkOrderDivergentCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            orderService.MarkOrderDivergentAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> RefuseOrderConfirmation(OrderDecisionRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new RefuseOrderConfirmationCommand(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.Id.ToGuid("id"),
                request.Note.ToOptionalString(request.HasNote) ?? string.Empty),
            orderService.RefuseOrderConfirmationAsync,
            context.CancellationToken)).ToContract();

    public override async Task<Order> ResolveOrderDivergence(OrderIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new ResolveOrderDivergenceCommand(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            orderService.ResolveOrderDivergenceAsync,
            context.CancellationToken)).ToContract();

    // ---------- Queries ----------

    public override async Task<Order> GetOrder(OrderIdRequest request, ServerCallContext context) =>
        (await validation.RunAsync(
            new GetOrderQuery(request.Context.ToUserId(), request.Context.ToOrganizationId(), request.Id.ToGuid("id")),
            orderService.GetOrderAsync,
            context.CancellationToken)).ToContract();

    public override async Task<ListOrdersResponse> ListOrders(ListOrdersRequest request, ServerCallContext context)
    {
        var (page, pageSize) = request.Page.ToPaging();
        var orders = await validation.RunAsync(
            new ListOrdersQuery(
                request.Context.ToUserId(),
                request.Context.ToOrganizationId(),
                request.MandateId.ToOptionalString(request.HasMandateId).ToOptionalGuid("mandate_id"),
                request.Approval.ToOptionalDomain<Entities.ApprovalStatus>("approval"),
                request.Confirmation.ToOptionalDomain<Entities.ConfirmationStatus>("confirmation"),
                page,
                pageSize,
                request.WithoutMandate,
                request.OnlyOutside),
            orderService.ListOrdersAsync,
            context.CancellationToken);

        return new ListOrdersResponse
        {
            Orders = { orders.Items.Select(o => o.ToContract()) },
            Page = orders.ToPageInfo(),
        };
    }
}

