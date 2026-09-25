using Fix.Domain.AggregateRoots.Organizations;

namespace Fix.Application.Organizations.Dtos;

public sealed record MemberDto(
    Guid Id,
    Guid UserId,
    string Email,
    string FullName,
    Desk? Desk,
    IReadOnlyList<string> Rules,
    IReadOnlyList<string> Groups);

