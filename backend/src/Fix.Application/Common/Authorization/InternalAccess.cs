using Fix.Domain.AggregateRoots.Organizations.Repositories;
using Fix.Domain.AggregateRoots.Rules;

namespace Fix.Application.Authorization;

/// <summary>Papel na equipe interna FIX (membro da organização FIX). Nunca é atribuído a usuários de clientes.</summary>
public enum InternalLevel
{
    None,
    Administrator,
    SuperAdministrator,
}

/// <summary>
/// Resolve se o usuário é da equipe interna FIX, consultando a membership na organização FIX (uma vez por requisição).
/// A equipe interna vê e edita as organizações clientes para dar suporte, sem ser membro delas.
/// </summary>
internal sealed class InternalAccess(IOrganizationRepository organizationRepository)
{
    private readonly Dictionary<Guid, InternalLevel> _cache = [];

    public async Task<InternalLevel> GetLevelAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(userId, out var cached))
        {
            return cached;
        }

        var level = await organizationRepository.GetInternalRuleCodeAsync(userId, cancellationToken) switch
        {
            RuleCodes.SuperAdministrador => InternalLevel.SuperAdministrator,
            RuleCodes.Administrador => InternalLevel.Administrator,
            _ => InternalLevel.None,
        };

        _cache[userId] = level;
        return level;
    }

    public static string? CodeOf(InternalLevel level) => level switch
    {
        InternalLevel.SuperAdministrator => RuleCodes.SuperAdministrador,
        InternalLevel.Administrator => RuleCodes.Administrador,
        _ => null,
    };
}
