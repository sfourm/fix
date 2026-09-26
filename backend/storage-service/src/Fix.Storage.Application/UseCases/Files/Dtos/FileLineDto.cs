using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Dtos;

public sealed record FileLineDto(
    Guid Id,
    int Number,
    FileLineStatus Status,
    // Coluna → valor, como veio no arquivo.
    IReadOnlyDictionary<string, string> Values,
    string? Message,
    string? ResultCode,
    DateTimeOffset? ProcessedAt);
