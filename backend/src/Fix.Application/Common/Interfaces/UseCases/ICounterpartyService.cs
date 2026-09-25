using Fix.Application.Counterparties.Commands;
using Fix.Application.Counterparties.Dtos;
using Fix.Application.Counterparties.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Contrapartes homologadas da companhia. A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface ICounterpartyService
{
    Task<CounterpartyDto> CreateCounterpartyAsync(CreateCounterpartyCommand command, CancellationToken cancellationToken);

    Task<CounterpartyDto> UpdateCounterpartyAsync(UpdateCounterpartyCommand command, CancellationToken cancellationToken);

    Task<CounterpartyDto> SetCounterpartyHomologationAsync(SetCounterpartyHomologationCommand command, CancellationToken cancellationToken);

    Task DeleteCounterpartyAsync(DeleteCounterpartyCommand command, CancellationToken cancellationToken);

    Task<CounterpartyDto> GetCounterpartyAsync(GetCounterpartyQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<CounterpartyDto>> ListCounterpartiesAsync(ListCounterpartiesQuery query, CancellationToken cancellationToken);
}
