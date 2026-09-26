using Fix.Application.Mandates.Mappers;
using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Dtos;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Orders;

namespace Fix.Application.Orders.Mappers;

internal static class OrderMapper
{
    public static OrderDto ToDto(this Order order, Mandate? mandate, string counterpartyName, DateOnly today) => new(
        order.Id,
        order.Code,
        order.MandateId,
        mandate?.Code,
        mandate?.Title.Value ?? string.Empty,
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
        order.CoveredSale,
        order.TradeDate,
        order.Notes,
        order.Compliance.ToDto(),
        order.LinkedAfterExecution,
        order.ExceedsMandate,
        order.DeviationNote,
        order.Approval,
        order.RequestedBy,
        order.DecidedBy,
        order.DecidedAt,
        order.DecisionNote,
        order.Confirmation,
        order.ConfirmedOn,
        order.ConfirmationNote,
        order.ConfirmationBy,
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
        input.Notes,
        input.Commodity,
        input.CoveredSale,
        input.Justification);
}
