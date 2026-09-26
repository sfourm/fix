using Fix.Contracts.V1;
using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Exceptions;
using Grpc.Core;

namespace Fix.Storage.Infrastructure.CoreService;

/// <summary>Regras efetivas do usuário (cargos diretos e dos grupos), calculadas pelo core. Cache por requisição.</summary>
internal sealed class RoleResolver(OrganizationService.OrganizationServiceClient organizations) : IRoleResolver
{
    private readonly Dictionary<(Guid, Guid), IReadOnlySet<string>> _cache = [];

    public async Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue((organizationId, userId), out var cached))
        {
            return cached;
        }

        try
        {
            var response = await organizations.GetUserRolesAsync(
                new GetUserRolesRequest { Context = new RequestContext { UserId = userId.ToString(), OrganizationId = organizationId.ToString() } },
                cancellationToken: cancellationToken);
            var roles = response.Roles.ToHashSet(StringComparer.Ordinal);
            _cache[(organizationId, userId)] = roles;
            return roles;
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.PermissionDenied or StatusCode.NotFound)
        {
            throw new ForbiddenException("O usuário não é membro desta organização.");
        }
    }
}
