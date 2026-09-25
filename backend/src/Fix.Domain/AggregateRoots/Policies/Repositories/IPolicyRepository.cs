using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Policies;

namespace Fix.Domain.AggregateRoots.Policies.Repositories;

public interface IPolicyRepository
{
    /// <summary>Carrega o agregado completo (eixos, bandas, instrumentos e versões).</summary>
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedList<Policy>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>Políticas vigentes da organização atual (no máximo uma, garantida pela aprovação).</summary>
    Task<IReadOnlyList<Policy>> ListActiveAsync(CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, Guid? exceptId, CancellationToken cancellationToken);

    Task<bool> HasMandatesAsync(Guid policyId, CancellationToken cancellationToken);

    Task<bool> AxisHasMandatesAsync(Guid axisId, CancellationToken cancellationToken);

    void Add(Policy policy);

    void Remove(Policy policy);
}
