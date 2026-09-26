using System.Collections.Concurrent;
using System.Reflection;
using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Exceptions;
using Fix.Storage.Application.Abstractions.Messaging;

namespace Fix.Storage.Application.Authorization;

/// <summary>
/// Portão de entrada de todo caso de uso: confere usuário/organização do contrato e, quando o caso de uso exige
/// (<see cref="RequireMembershipAttribute"/>), que o usuário é membro da organização. A regra por tipo de arquivo
/// fica em <see cref="FilePermissions"/>.
/// </summary>
internal sealed class UseCaseGuard(IRoleResolver roleResolver)
{
    private static readonly ConcurrentDictionary<Type, bool> RequiresMembership = new();

    public async Task EnterAsync(IUseCase useCase, CancellationToken cancellationToken)
    {
        if (useCase is IUserRequest { UserId: var userId } && userId == Guid.Empty)
        {
            throw new BadRequestException("'user_id' é obrigatório.");
        }

        if (useCase is IOrganizationRequest { OrganizationId: var organizationId } && organizationId == Guid.Empty)
        {
            throw new BadRequestException("'organization_id' é obrigatório.");
        }

        if (!RequiresMembership.GetOrAdd(useCase.GetType(), type => type.GetCustomAttribute<RequireMembershipAttribute>() is not null))
        {
            return;
        }

        if (useCase is not IOrganizationRequest request)
        {
            throw new InvalidOperationException(
                $"{useCase.GetType().Name} exige autorização e deve implementar {nameof(IOrganizationRequest)}.");
        }

        var roles = await roleResolver.GetRolesAsync(request.OrganizationId, request.UserId, cancellationToken);
        if (roles.Count == 0)
        {
            throw new ForbiddenException("O usuário não é membro desta organização.");
        }
    }
}
