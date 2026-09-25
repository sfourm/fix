using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Dtos;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Application.Orders.Mappers;

internal static class OrderMapper
{
    public static OrderDto ToDto(this Order order, string mandateTitle, string counterpartyName, DateOnly today) => new(
        order.Id,
        order.MandateId,
        mandateTitle,
        order.Type,
        order.Direction,
        order.CounterpartyId,
        counterpartyName,
        order.Commodity,
        order.Tenor.Code,
        order.Lots,
        order.NotionalUsd,
        order.Price,
        order.PriceUnit,
        order.OptionKind,
        order.Premium,
        order.TradeDate,
        order.Notes,
        order.Approval,
        order.RequestedBy,
        order.DecidedBy,
        order.DecidedAt,
        order.DecisionNote,
        order.Confirmation,
        order.ConfirmedOn,
        order.ConfirmationNote,
        order.IsConfirmationOverdue(today));

    public static OrderTerms ToTerms(this OrderTermsInput input) => new(
        input.Type,
        input.Direction,
        Tenor.Create(input.Tenor),
        input.Lots,
        input.NotionalUsd,
        input.Price,
        input.PriceUnit,
        input.OptionKind,
        input.Premium,
        input.TradeDate,
        input.Notes);
}

