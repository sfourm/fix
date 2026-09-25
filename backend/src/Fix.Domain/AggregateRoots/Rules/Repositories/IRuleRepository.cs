using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Domain.AggregateRoots.Rules.Repositories;

public interface IRuleRepository
{
    /// <summary>Rules visíveis na organização: as de sistema e as alçadas personalizadas dela.</summary>
    Task<IReadOnlyList<Rule>> ListForOrganizationAsync(Guid organizationId, CancellationToken cancellationToken);

    Task<Rule?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Alçadas personalizadas da organização pelos códigos (códigos desconhecidos ficam de fora).</summary>
    Task<IReadOnlyList<Rule>> ListAlcadasByCodeAsync(
        Guid organizationId,
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(Guid organizationId, string code, Guid? exceptId, CancellationToken cancellationToken);

    void Add(Rule rule);

    void Remove(Rule rule);
}
