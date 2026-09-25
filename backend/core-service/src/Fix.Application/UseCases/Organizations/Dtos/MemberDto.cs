using Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Application.Organizations.Dtos;

public sealed record MemberDto(
    Guid Id,
    Guid UserId,
    string Email,
    string FullName,
    Desk? Desk,
    // Rule de base: owner, user ou, na organização FIX, super_administrador/administrador.
    string Role,
    bool IsOwner,
    // Códigos das alçadas atribuídas diretamente ao membro.
    IReadOnlyList<string> Rules,
    IReadOnlyList<string> Groups);

