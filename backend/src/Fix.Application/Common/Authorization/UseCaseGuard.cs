using System.Collections.Concurrent;
using System.Reflection;
using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Organizations.Repositories;

namespace Fix.Application.Authorization;

/// <summary>
/// Portão de entrada de todo caso de uso: copia usuário/organização do contrato para o <see cref="IRequestContext"/>
/// e valida, consultando a base, membership e roles exigidas por <see cref="RequireMembershipAttribute"/>/<see cref="RequireRoleAttribute"/>.
/// </summary>
internal sealed class UseCaseGuard(
    RequestContext requestContext,
    IOrganizationRepository organizationRepository,
    IRoleResolver roleResolver)
{
    private static readonly ConcurrentDictionary<Type, RequireMembershipAttribute?> Requirements = new();

    public async Task EnterAsync(IUseCase useCase, CancellationToken cancellationToken)
    {
        SetContext(useCase);

        var requirement = Requirements.GetOrAdd(useCase.GetType(), type => type.GetCustomAttribute<RequireMembershipAttribute>());
        if (requirement is null)
        {
            return;
        }

        if (useCase is not IOrganizationRequest request)
        {
            throw new InvalidOperationException(
                $"{useCase.GetType().Name} exige autorização e deve implementar {nameof(IOrganizationRequest)}.");
        }

        var roles = await roleResolver.GetRolesAsync(request.OrganizationId, request.UserId, cancellationToken);

        if (requirement is RequireRoleAttribute required)
        {
            if (!roles.Contains(required.Role))
            {
                throw new ForbiddenException($"Role '{required.Role}' necessária.");
            }
        }
        else if (roles.Count == 0 && !await organizationRepository.IsMemberAsync(request.OrganizationId, request.UserId, cancellationToken))
        {
            throw new ForbiddenException("O usuário não é membro desta organização.");
        }
    }

    private void SetContext(IUseCase useCase)
    {
        if (useCase is not IUserRequest userRequest)
        {
            return;
        }

        if (userRequest.UserId == Guid.Empty)
        {
            throw new BadRequestException("'user_id' é obrigatório.");
        }

        Guid? organizationId = null;
        if (useCase is IOrganizationRequest organizationRequest)
        {
            organizationId = organizationRequest.OrganizationId != Guid.Empty
                ? organizationRequest.OrganizationId
                : throw new BadRequestException("'organization_id' é obrigatório.");
        }

        requestContext.Set(userRequest.UserId, organizationId);
    }
}
