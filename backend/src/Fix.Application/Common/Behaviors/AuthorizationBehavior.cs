using System.Reflection;
using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Organizations.Repositories;

namespace Fix.Application.Behaviors;

/// <summary>
/// Valida, consultando a base, se o usuário do contrato pode executar a ação na organização:
/// membership e roles (diretas, dos grupos ou da rule de plataforma super_administrador).
/// </summary>
internal sealed class AuthorizationBehavior<TRequest, TResult>(
    IOrganizationRepository organizationRepository,
    IRoleResolver roleResolver)
    : IPipelineBehavior<TRequest, TResult>
    where TRequest : notnull
{
    private static readonly RequireMembershipAttribute? Requirement =
        typeof(TRequest).GetCustomAttribute<RequireMembershipAttribute>();

    public async Task<TResult> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken)
    {
        if (Requirement is null)
        {
            return await next();
        }

        if (request is not IOrganizationRequest organizationRequest)
        {
            throw new InvalidOperationException(
                $"{typeof(TRequest).Name} exige autorização e deve implementar {nameof(IOrganizationRequest)}.");
        }

        var (userId, organizationId) = (organizationRequest.UserId, organizationRequest.OrganizationId);
        var roles = await roleResolver.GetRolesAsync(organizationId, userId, cancellationToken);

        if (Requirement is RequireRoleAttribute required)
        {
            if (!roles.Contains(required.Role))
            {
                throw new ForbiddenException($"Role '{required.Role}' necessária.");
            }
        }
        else if (roles.Count == 0 && !await organizationRepository.IsMemberAsync(organizationId, userId, cancellationToken))
        {
            throw new ForbiddenException("O usuário não é membro desta organização.");
        }

        return await next();
    }
}
