using Fix.Domain.Common;
using Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Domain.AggregateRoots.Organizations.Repositories;

public interface IOrganizationRepository
{
    /// <summary>Carrega o agregado completo (membros, grupos, rules e commodities).</summary>
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Organization>> ListByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> SlugExistsAsync(Slug slug, CancellationToken cancellationToken);

    Task<bool> IsMemberAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Roles efetivas do usuário na organização (rules diretas + rules herdadas dos grupos).</summary>
    Task<IReadOnlyCollection<string>> GetRoleCodesAsync(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken);

    void Add(Organization organization);
}
