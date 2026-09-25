using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Counterparties.Commands;
using Fix.Application.Counterparties.Dtos;
using Fix.Application.Counterparties.Mappers;
using Fix.Application.Counterparties.Queries;
using Fix.Domain.Abstractions;
using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Counterparties;
using Fix.Domain.AggregateRoots.Counterparties.Repositories;

namespace Fix.Application.Counterparties.Services;

internal sealed class CounterpartyService(ICounterpartyRepository counterpartyRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateCounterpartyCommand, CounterpartyDto>,
      ICommandHandler<UpdateCounterpartyCommand, CounterpartyDto>,
      ICommandHandler<SetCounterpartyHomologationCommand, CounterpartyDto>,
      ICommandHandler<DeleteCounterpartyCommand, Unit>,
      IQueryHandler<GetCounterpartyQuery, CounterpartyDto>,
      IQueryHandler<ListCounterpartiesQuery, IReadOnlyList<CounterpartyDto>>
{
    // ---------- Commands ----------

    public async Task<CounterpartyDto> HandleAsync(CreateCounterpartyCommand command, CancellationToken cancellationToken)
    {
        var name = Name.Create(command.Name);
        await EnsureUniqueNameAsync(name, null, cancellationToken);

        var counterparty = Counterparty.Create(
            command.OrganizationId,
            name,
            command.Type,
            command.Document,
            command.Address,
            command.Country,
            command.NotionalLimitUsd,
            command.MtmLimitUsd);

        counterpartyRepository.Add(counterparty);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return counterparty.ToDto();
    }

    public async Task<CounterpartyDto> HandleAsync(UpdateCounterpartyCommand command, CancellationToken cancellationToken)
    {
        var counterparty = await GetAsync(command.Id, cancellationToken);
        var name = Name.Create(command.Name);
        await EnsureUniqueNameAsync(name, counterparty.Id, cancellationToken);

        counterparty.Update(
            name,
            command.Type,
            command.Document,
            command.Address,
            command.Country,
            command.NotionalLimitUsd,
            command.MtmLimitUsd);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return counterparty.ToDto();
    }

    public async Task<CounterpartyDto> HandleAsync(SetCounterpartyHomologationCommand command, CancellationToken cancellationToken)
    {
        var counterparty = await GetAsync(command.Id, cancellationToken);

        counterparty.SetHomologation(command.Homologated);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return counterparty.ToDto();
    }

    public async Task<Unit> HandleAsync(DeleteCounterpartyCommand command, CancellationToken cancellationToken)
    {
        var counterparty = await GetAsync(command.Id, cancellationToken);

        var usages = await counterpartyRepository.CountUsagesAsync(counterparty.Id, cancellationToken);
        if (usages > 0)
        {
            throw new DomainException(
                $"{counterparty.Name} tem {usages} boleta(s) vinculada(s) — desomologue para bloquear novas operações.");
        }

        counterpartyRepository.Remove(counterparty);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    // ---------- Queries ----------

    public async Task<CounterpartyDto> HandleAsync(GetCounterpartyQuery query, CancellationToken cancellationToken) =>
        (await GetAsync(query.Id, cancellationToken)).ToDto();

    public async Task<IReadOnlyList<CounterpartyDto>> HandleAsync(ListCounterpartiesQuery query, CancellationToken cancellationToken)
    {
        var counterparties = await counterpartyRepository.ListAsync(query.OnlyHomologated, cancellationToken);
        return counterparties.Select(c => c.ToDto()).ToList();
    }

    // ---------- Helpers ----------

    private async Task<Counterparty> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await counterpartyRepository.GetByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Contraparte", id);

    private async Task EnsureUniqueNameAsync(Name name, Guid? exceptId, CancellationToken cancellationToken)
    {
        if (await counterpartyRepository.NameExistsAsync(name.Value, exceptId, cancellationToken))
        {
            throw new ConflictException($"Já existe uma contraparte chamada '{name}'.");
        }
    }
}

