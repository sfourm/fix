using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Mandates;

namespace Fix.Domain.AggregateRoots.Mandates.Repositories;

public sealed record MandateFilter(Guid? PolicyId, MandateStatus? Status);

public interface IMandateRepository
{
    Task<Mandate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<Mandate>> ListAsync(MandateFilter filter, int page, int pageSize, CancellationToken cancellationToken);

    Task<bool> HasOrdersAsync(Guid mandateId, CancellationToken cancellationToken);

    /// <summary>Quantidade consumida por boletas aprovadas (lotes ou US$), por mandato.</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetConsumedAsync(
        IReadOnlyCollection<Guid> mandateIds,
        Guid? exceptOrderId,
        CancellationToken cancellationToken);

    void Add(Mandate mandate);

    void Remove(Mandate mandate);
}
