namespace Fix.Application.Organizations.Dtos;

public sealed record GroupDto(
    Guid Id,
    string Name,
    bool IsDefault,
    IReadOnlyList<string> Rules,
    IReadOnlyList<Guid> MemberIds);
