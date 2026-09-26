using Fix.Application.Counterparties.Dtos;
using Fix.Domain.AggregateRoots.Counterparties;

namespace Fix.Application.Counterparties.Mappers;

internal static class CounterpartyMapper
{
    public static CounterpartyDto ToDto(this Counterparty counterparty) => new(
        counterparty.Id,
        counterparty.Code,
        counterparty.Name.Value,
        counterparty.Type,
        counterparty.Document,
        counterparty.Address,
        counterparty.Country,
        counterparty.IsHomologated,
        counterparty.NotionalLimitUsd,
        counterparty.MtmLimitUsd);
}

