namespace Fix.Application.Rules.Dtos;

/// <summary>Rule: conjunto de roles atribuído a membros e grupos.</summary>
public sealed record RuleDto(Guid Id, string Code, string Name, IReadOnlyList<string> Roles);
