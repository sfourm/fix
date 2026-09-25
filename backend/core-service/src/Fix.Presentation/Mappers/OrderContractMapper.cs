using Fix.Application.Orders.Commands;
using Fix.Application.Orders.Dtos;
using Contract = Fix.Contracts.V1;
using Entities = Fix.Domain.AggregateRoots.Orders;

namespace Fix.Presentation.Mappers;

internal static class OrderContractMapper
{
    public static Contract.Order ToContract(this OrderDto order)
    {
        var terms = new Contract.OrderTerms
        {
            Type = order.Type.ToContract<Contract.OrderType>(),
            Direction = order.Direction.ToContract<Contract.TradeDirection>(),
            Tenor = order.Tenor,
            Price = (double)order.Price,
            PriceUnit = order.PriceUnit,
            OptionKind = order.OptionKind.ToContract<Contract.OptionKind>(),
            TradeDate = order.TradeDate.ToContract(),
        };

        if (order.Lots is { } lots) terms.Lots = (double)lots;
        if (order.NotionalUsd is { } notional) terms.NotionalUsd = (double)notional;
        if (order.Premium is { } premium) terms.Premium = (double)premium;
        if (order.Notes is { } notes) terms.Notes = notes;

        var contract = new Contract.Order
        {
            Id = order.Id.ToString(),
            MandateId = order.MandateId.ToString(),
            MandateTitle = order.MandateTitle,
            CounterpartyId = order.CounterpartyId.ToString(),
            CounterpartyName = order.CounterpartyName,
            Commodity = order.Commodity.ToContract<Contract.Commodity>(),
            Terms = terms,
            Approval = order.Approval.ToContract<Contract.ApprovalStatus>(),
            RequestedBy = order.RequestedBy.ToString(),
            Confirmation = order.Confirmation.ToContract<Contract.ConfirmationStatus>(),
            ConfirmationOverdue = order.ConfirmationOverdue,
        };

        if (order.DecidedBy is { } decidedBy) contract.DecidedBy = decidedBy.ToString();
        if (order.DecidedAt is { } decidedAt) contract.DecidedAt = decidedAt.ToContract();
        if (order.DecisionNote is { } note) contract.DecisionNote = note;
        if (order.ConfirmedOn is { } confirmedOn) contract.ConfirmedOn = confirmedOn.ToContract();
        if (order.ConfirmationNote is { } confirmationNote) contract.ConfirmationNote = confirmationNote;
        return contract;
    }

    public static OrderTermsInput ToInput(this Contract.OrderTerms? terms)
    {
        terms ??= new Contract.OrderTerms();
        return new OrderTermsInput(
            terms.Type.ToDomain<Entities.OrderType>("terms.type"),
            terms.Direction.ToDomain<Entities.TradeDirection>("terms.direction"),
            terms.Tenor,
            terms.Lots.ToOptionalDecimal(terms.HasLots),
            terms.NotionalUsd.ToOptionalDecimal(terms.HasNotionalUsd),
            terms.Price.ToDecimal(),
            terms.PriceUnit.ToOptionalString(terms.HasPriceUnit),
            terms.OptionKind.ToOptionalDomain<Entities.OptionKind>("terms.option_kind"),
            terms.Premium.ToOptionalDecimal(terms.HasPremium),
            terms.TradeDate.ToDate("terms.trade_date"),
            terms.Notes.ToOptionalString(terms.HasNotes));
    }
}

