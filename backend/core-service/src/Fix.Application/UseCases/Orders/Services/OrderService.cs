using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Authorization;
using Fix.Application.Common;
using Fix.Application.Common.Interfaces.UseCases;
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
    OrgChartApproval orgChart,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
    : IOrderService
{
    // ---------- Commands ----------

    /// <summary>
    /// Registra a boleta (com ou sem mandato). Dentro do enquadramento e com alçada (self_approve) já nasce aprovada e
    /// consome saldo; desvios exigem justificativa e vão para a fila (FIX2 · I-01).
    /// </summary>
    public async Task<OrderDto> RegisterOrderAsync(RegisterOrderCommand command, CancellationToken cancellationToken)
    {
        var mandate = command.MandateId is { } mandateId ? await GetMandateAsync(mandateId, cancellationToken) : null;
        var counterparty = await GetCounterpartyAsync(command.CounterpartyId, cancellationToken);
        var consumed = mandate is null ? 0 : await ConsumedAsync(mandate.Id, null, cancellationToken);
        var roles = await roleResolver.GetRolesAsync(command.OrganizationId, command.UserId, cancellationToken);

        var order = Order.Register(
            await orderRepository.NextNumberAsync(cancellationToken),
            mandate,
            counterparty,
            command.Terms.ToTerms(),
            consumed,
            command.UserId,
            roles.Contains(RoleCodes.SelfApprove));

        orderRepository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto(mandate, counterparty.Name.Value, timeProvider.Today());
    }

    public async Task<OrderDto> UpdateOrderAsync(UpdateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        var mandate = await FindMandateAsync(order.MandateId, cancellationToken);
        var counterparty = await GetCounterpartyAsync(command.CounterpartyId, cancellationToken);
        var consumed = mandate is null ? 0 : await ConsumedAsync(mandate.Id, order.Id, cancellationToken);

        order.Update(mandate, counterparty, command.Terms.ToTerms(), consumed);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToDto(mandate, counterparty.Name.Value, timeProvider.Today());
    }

    public async Task<OrderDto> LinkOrderMandateAsync(LinkOrderMandateCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        var mandate = await GetMandateAsync(command.MandateId, cancellationToken);
        var consumed = await ConsumedAsync(mandate.Id, order.Id, cancellationToken);

        order.LinkMandate(mandate, consumed, command.Justification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(order, mandate, cancellationToken);
    }

    /// <summary>Na aprovação o enquadramento é refeito, pois outras boletas podem ter consumido o mandato.</summary>
    public async Task<OrderDto> ApproveOrderAsync(ApproveOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        await orgChart.EnsureCanDecideAsync(command.OrganizationId, command.UserId, order.RequestedBy, cancellationToken);

        var mandate = await FindMandateAsync(order.MandateId, cancellationToken);
        var consumed = mandate is null ? 0 : await ConsumedAsync(mandate.Id, order.Id, cancellationToken);

        order.Approve(mandate, consumed, command.UserId, timeProvider.GetUtcNow(), command.Note);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ToDtoAsync(order, mandate, cancellationToken);
    }

    public async Task<OrderDto> RejectOrderAsync(RejectOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        await orgChart.EnsureCanDecideAsync(command.OrganizationId, command.UserId, order.RequestedBy, cancellationToken);

        return await ChangeAsync(command.Id, o => o.Reject(command.UserId, timeProvider.GetUtcNow(), command.Reason), cancellationToken);
    }

    public async Task DeleteOrderAsync(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await GetAsync(command.Id, cancellationToken);
        if (!order.CanBeDeleted)
        {
            throw new DomainException("Boletas aprovadas não podem ser excluídas — elas já consumiram o mandato.");
        }

        orderRepository.Remove(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ---------- Commands: confirmation (middle office; quem executou não confere) ----------

    public Task<OrderDto> ConfirmOrderAsync(ConfirmOrderCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.Confirm(command.ReceivedOn, command.UserId), cancellationToken);

    public Task<OrderDto> MarkOrderDivergentAsync(MarkOrderDivergentCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.MarkDivergent(command.Description, command.UserId), cancellationToken);

    public Task<OrderDto> RefuseOrderConfirmationAsync(RefuseOrderConfirmationCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.RefuseConfirmation(command.Reason, command.UserId), cancellationToken);

    public Task<OrderDto> ResolveOrderDivergenceAsync(ResolveOrderDivergenceCommand command, CancellationToken cancellationToken) =>
        ChangeAsync(command.Id, o => o.ResolveDivergence(timeProvider.Today(), command.UserId), cancellationToken);

    // ---------- Queries ----------

    public async Task<OrderDto> GetOrderAsync(GetOrderQuery query, CancellationToken cancellationToken) =>
        await ToDtoAsync(await GetAsync(query.Id, cancellationToken), null, cancellationToken);

    public async Task<PagedList<OrderDto>> ListOrdersAsync(ListOrdersQuery query, CancellationToken cancellationToken)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        var orders = await orderRepository.ListAsync(
            new OrderFilter(query.MandateId, query.Approval, query.Confirmation, query.WithoutMandate, query.OnlyOutside),
            page,
            pageSize,
            cancellationToken);

        var counterparties = (await counterpartyRepository.ListAsync(onlyHomologated: false, cancellationToken))
            .ToDictionary(c => c.Id, c => c.Name.Value);
        var mandates = new Dictionary<Guid, Mandate?>();
        foreach (var mandateId in orders.Items.Select(o => o.MandateId).OfType<Guid>().Distinct())
        {
            mandates[mandateId] = await mandateRepository.GetByIdAsync(mandateId, cancellationToken);
        }

        var today = timeProvider.Today();
        return orders.Map(o => o.ToDto(
            o.MandateId is { } id ? mandates.GetValueOrDefault(id) : null,
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
        mandate ??= await FindMandateAsync(order.MandateId, cancellationToken);
        var counterparty = await counterpartyRepository.GetByIdAsync(order.CounterpartyId, cancellationToken);
        return order.ToDto(mandate, counterparty?.Name.Value ?? string.Empty, timeProvider.Today());
    }

    private async Task<decimal> ConsumedAsync(Guid mandateId, Guid? exceptOrderId, CancellationToken cancellationToken) =>
        (await mandateRepository.GetConsumedAsync([mandateId], exceptOrderId, cancellationToken)).GetValueOrDefault(mandateId);

    private async Task<Order> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await orderRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Boleta", id);

    private async Task<Mandate> GetMandateAsync(Guid id, CancellationToken cancellationToken) =>
        await mandateRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Mandato", id);

    private async Task<Mandate?> FindMandateAsync(Guid? id, CancellationToken cancellationToken) =>
        id is { } mandateId ? await GetMandateAsync(mandateId, cancellationToken) : null;

    private async Task<Counterparty> GetCounterpartyAsync(Guid id, CancellationToken cancellationToken) =>
        await counterpartyRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Contraparte", id);
}
