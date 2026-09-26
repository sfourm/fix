using Fix.Application.Counterparties.Dtos;
using Contract = Fix.Contracts.V1;

namespace Fix.Presentation.Mappers;

internal static class CounterpartyContractMapper
{
    public static Contract.Counterparty ToContract(this CounterpartyDto counterparty)
    {
        var contract = new Contract.Counterparty
        {
            Id = counterparty.Id.ToString(),
            Code = counterparty.Code,
            Name = counterparty.Name,
            Type = counterparty.Type.ToContract<Contract.CounterpartyType>(),
            IsHomologated = counterparty.IsHomologated,
        };

        if (counterparty.Document is { } document) contract.Document = document;
        if (counterparty.Address is { } address) contract.Address = address;
        if (counterparty.Country is { } country) contract.Country = country;
        if (counterparty.NotionalLimitUsd is { } notional) contract.NotionalLimitUsd = (double)notional;
        if (counterparty.MtmLimitUsd is { } mtm) contract.MtmLimitUsd = (double)mtm;
        return contract;
    }
}
