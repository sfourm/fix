using Fix.Application.Authorization;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Abstractions.Authorization;

/// <summary>
/// Roles efetivas do usuário na organização, consultadas na base: as da rule de base (owner/user), das alçadas
/// diretas e das alçadas dos grupos. Numa organização cliente, a equipe interna FIX recebe ainda as roles de suporte
/// (ver e editar, sem as decisões), mesmo sem ser membro.
/// </summary>
public interface IRoleResolver
{
    Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);
}

internal sealed class RoleResolver(IOrganizationRepository organizationRepository, InternalAccess internalAccess)
    : IRoleResolver
{
    private readonly Dictionary<(Guid, Guid), IReadOnlySet<string>> _cache = [];

    public async Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue((organizationId, userId), out var cached))
        {
            return cached;
        }

        var roles = (await organizationRepository.GetRoleCodesAsync(organizationId, userId, cancellationToken)).ToHashSet();

        if (organizationId != Organization.InternalOrganizationId
            && await internalAccess.GetLevelAsync(userId, cancellationToken) != InternalLevel.None
            && await organizationRepository.ExistsAsync(organizationId, cancellationToken))
        {
            roles.UnionWith(SystemRoles.Staff);
        }

        _cache[(organizationId, userId)] = roles;
        return roles;
    }
}
