using Fix.Domain.AggregateRoots.Counterparties;

namespace Fix.Domain.AggregateRoots.Counterparties.Repositories;

public interface ICounterpartyRepository
{
    Task<Counterparty?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Counterparty>> ListAsync(bool onlyHomologated, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    /// <summary>Quantidade de boletas que usam a contraparte (impede a exclusão).</summary>
    Task<int> CountUsagesAsync(Guid counterpartyId, CancellationToken cancellationToken);

    /// <summary>Próximo número sequencial de contraparte da organização (CP-01...).</summary>
    Task<int> NextNumberAsync(CancellationToken cancellationToken);

    void Add(Counterparty counterparty);

    void Remove(Counterparty counterparty);
}
