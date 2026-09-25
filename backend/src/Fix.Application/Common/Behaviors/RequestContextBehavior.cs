using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;

namespace Fix.Application.Behaviors;

/// <summary>Copia o usuário/organização do contrato para o <see cref="IRequestContext"/> da requisição.</summary>
internal sealed class RequestContextBehavior<TRequest, TResult>(RequestContext requestContext)
    : IPipelineBehavior<TRequest, TResult>
    where TRequest : notnull
{
    public Task<TResult> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken)
    {
        if (request is IUserRequest userRequest)
        {
            if (userRequest.UserId == Guid.Empty)
            {
                throw new BadRequestException("'user_id' é obrigatório.");
            }

            Guid? organizationId = null;
            if (request is IOrganizationRequest organizationRequest)
            {
                organizationId = organizationRequest.OrganizationId != Guid.Empty
                    ? organizationRequest.OrganizationId
                    : throw new BadRequestException("'organization_id' é obrigatório.");
            }

            requestContext.Set(userRequest.UserId, organizationId);
        }

        return next();
    }
}
