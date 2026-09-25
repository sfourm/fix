using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Common;
using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Dtos;
using Fix.Application.Orders.Mappers;
using Fix.Application.Orders.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Counterparties.Repositories;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Mandates.Repositories;
using Fix.Domain.AggregateRoots.Orders;
using Fix.Domain.AggregateRoots.Orders.Repositories;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Orders.Services;

internal sealed class OrderService(
    IOrderRepository orderRepository,
    IMandateRepository mandateRepository,
    ICounterpartyRepository counterpartyRepository,
    IRoleResolver roleResolver,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RegisterOrderCommand, OrderDto>,
      ICommandHandler<UpdateOrderCommand, OrderDto>,
      ICommandHandler<ApproveOrderCommand, OrderDto>,
      ICommandHandler<RejectOrderCommand, OrderDto>,
      ICommandHandler<DeleteOrderCommand, Unit>,
      ICommandHandler<ConfirmOrderCommand, OrderDto>,
      ICommandHandler<MarkOrderDivergentCommand, OrderDto>,
      ICommandHandler<RefuseOrderConfirmationCommand, OrderDto>,
      ICommandHandler<ResolveOrderDivergenceCommand, OrderDto>,
      IQueryHandler<GetOrderQuery, OrderDto>,
      IQueryHandler<ListOrdersQuery, PagedList<OrderDto>>
{
    // ---------- Commands ----------

    /// <summary>Registra a boleta no mandato ativo; com alçada (self_approve) já nasce aprovada e consome saldo.</summary>
    public async Task<OrderDto> HandleAsync(RegisterOrderCommand command, CancellationToken cancellationToken)
    {
        var mandate = await GetMandateAsync(command.MandateId, cancellationToken);
        var counterparty = await GetCounterpartyAsync(command.CounterpartyId, cancellationToken);
        var consumed = await ConsumedAsync(mandate.Id, null, cancellationToken);
        var roles = await roleResolver.GetRolesAsync(command.OrganizationId, command.UserId, cancellationToken);

        var order = Order.Register(
            mandate,
            counterparty,
            command.Terms.ToTerms(),
            consumed,
            command.UserId,
            roles.Contains(RoleCodes.SelfApprove));

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto(mandate.Title.Value, counterparty.Name.Value, timeProvider.Today());
    }

    public async Task<OrderDto> HandleAsync(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        var mandate = await GetMandateAsync(order.MandateId, cancellationToken);
        var counterparty = await GetCounterpartyAsync(command.CounterpartyId, cancellationToken);
        var consumed = await ConsumedAsync(mandate.Id, order.Id, cancellationToken);

        order.Update(mandate, counterparty, command.Terms.ToTerms(), consumed);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto(mandate.Title.Value, counterparty.Name.Value, timeProvider.Today());
    }

    /// <summary>Na aprovação o saldo é conferido de novo, pois outras boletas podem ter consumido o mandato.</summary>
    public async Task<OrderDto> HandleAsync(ApproveOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        var mandate = await GetMandateAsync(order.MandateId, cancellationToken);
        var authorized = mandate.Quantity;
        if (authorized is not null)
        {
            var consumed = await ConsumedAsync(mandate.Id, order.Id, cancellationToken);
            if (consumed + order.Quantity > authorized)
            {
                throw new DomainException($"Aprovar a boleta excederia o saldo do mandato (saldo {authorized - consumed:N0}).");
            }
        }

        order.Approve(command.UserId, timeProvider.GetUtcNow(), command.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(order, mandate, cancellationToken);
    }

    public Task<OrderDto> HandleAsync(RejectOrderCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.Reject(command.UserId, timeProvider.GetUtcNow(), command.Reason), cancellationToken);

    public async Task<Unit> HandleAsync(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        if (!order.CanBeDeleted)
        {
            throw new DomainException("Boletas aprovadas não podem ser excluídas — elas já consumiram o mandato.");
        }

        orderRepository.Remove(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    // ---------- Commands: confirmation ----------

    public Task<OrderDto> HandleAsync(ConfirmOrderCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.Confirm(command.ReceivedOn), cancellationToken);

    public Task<OrderDto> HandleAsync(MarkOrderDivergentCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.MarkDivergent(command.Description), cancellationToken);

    public Task<OrderDto> HandleAsync(RefuseOrderConfirmationCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.RefuseConfirmation(command.Reason), cancellationToken);

    public Task<OrderDto> HandleAsync(ResolveOrderDivergenceCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.ResolveDivergence(timeProvider.Today()), cancellationToken);

    // ---------- Queries ----------

    public async Task<OrderDto> HandleAsync(GetOrderQuery query, CancellationToken cancellationToken) =>
        await ToDtoAsync(await GetAsync(query.Id, cancellationToken), null, cancellationToken);

    public async Task<PagedList<OrderDto>> HandleAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var orders = await orderRepository.ListAsync(
            new OrderFilter(query.MandateId, query.Approval, query.Confirmation),
            page,
            pageSize,
            cancellationToken);

        var counterparties = (await counterpartyRepository.ListAsync(onlyHomologated: false, cancellationToken))
            .ToDictionary(c => c.Id, c => c.Name.Value);
        var mandates = new Dictionary<Guid, string>();
        foreach (var mandateId in orders.Items.Select(o => o.MandateId).Distinct())
        {
            mandates[mandateId] = (await mandateRepository.GetByIdAsync(mandateId, cancellationToken))?.Title.Value ?? string.Empty;
        }

        var today = timeProvider.Today();
        return orders.Map(o => o.ToDto(
            mandates.GetValueOrDefault(o.MandateId, string.Empty),
            counterparties.GetValueOrDefault(o.CounterpartyId, string.Empty),
            today));
    }

    // ---------- Helpers ----------

    private async Task<OrderDto> ChangeAsync(Guid id, Action<Order> change, CancellationToken cancellationToken)
    {
        var order = await GetAsync(id, cancellationToken);

        change(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(order, null, cancellationToken);
    }

    private async Task<OrderDto> ToDtoAsync(Order order, Mandate? mandate, CancellationToken cancellationToken)
    {
        mandate ??= await mandateRepository.GetByIdAsync(order.MandateId, cancellationToken);
        var counterparty = await counterpartyRepository.GetByIdAsync(order.CounterpartyId, cancellationToken);
        return order.ToDto(mandate?.Title.Value ?? string.Empty, counterparty?.Name.Value ?? string.Empty, timeProvider.Today());
    }

    private async Task<decimal> ConsumedAsync(Guid mandateId, Guid? exceptOrderId, CancellationToken cancellationToken) =>
        (await mandateRepository.GetConsumedAsync([mandateId], exceptOrderId, cancellationToken)).GetValueOrDefault(mandateId);

    private async Task<Order> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await orderRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Boleta", id);

    private async Task<Mandate> GetMandateAsync(Guid id, CancellationToken cancellationToken) =>
        await mandateRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Mandato", id);

    private async Task<Counterparty> GetCounterpartyAsync(Guid id, CancellationToken cancellationToken) =>
        await counterpartyRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Contraparte", id);
}

