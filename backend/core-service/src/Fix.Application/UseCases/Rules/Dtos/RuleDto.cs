namespace Fix.Application.Rules.Dtos;

/// <summary>Rule: conjunto de roles. Sistema (owner, user, internas) é fixa; alçada personalizada é editável pela organização.</summary>
public sealed record RuleDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<string> Roles,
    bool IsSystem,
    // Quantos membros e grupos usam a rule na organização.
    int Usages);
