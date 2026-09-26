namespace Fix.Storage.Application.Abstractions.Authorization;

/// <summary>
/// Regras efetivas do usuário na organização (cargos diretos e dos grupos), calculadas pelo core-service. Lança
/// <see cref="Exceptions.ForbiddenException"/> quando o usuário não é membro.
/// </summary>
public interface IRoleResolver
{
    Task<IReadOnlySet<string>> GetRolesAsync(Guid organizationId, Guid userId, CancellationToken cancellationToken);
}
