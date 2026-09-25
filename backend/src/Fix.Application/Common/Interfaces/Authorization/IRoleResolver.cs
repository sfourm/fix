using Fix.Application.Abstractions.Authentication;
using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Abstractions.Authorization;

/// <summary>Roles efetivas do usuário na organização, consultadas na base (super administrador tem todas).</summary>
public interface IRoleResolver
{
    Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);
}

internal sealed class RoleResolver(IOrganizationRepository organizationRepository, IIdentityService identityService)
    : IRoleResolver
{
    private readonly Dictionary<(Guid, Guid), IReadOnlySet<string>> _cache = [];

    public async Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue((organizationId, userId), out var cached))
        {
            return cached;
        }

        IReadOnlySet<string> roles = await identityService.IsSuperAdministratorAsync(userId, cancellationToken)
            ? SystemRoles.All.ToHashSet()
            : (await organizationRepository.GetRoleCodesAsync(organizationId, userId, cancellationToken)).ToHashSet();

        _cache[(organizationId, userId)] = roles;
        return roles;
    }
}

