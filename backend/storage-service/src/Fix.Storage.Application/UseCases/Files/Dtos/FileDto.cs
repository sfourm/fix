using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Dtos;

public sealed record FileDto(
    Guid Id,
    FileKind Kind,
    FileStatus Status,
    string FileName,
    string ContentType,
    long SizeBytes,
    Guid OrganizationId,
    Guid UploadedBy,
    DateTimeOffset UploadedAt,
    DateTimeOffset? FinishedAt,
    int TotalLines,
    int ProcessedLines,
    int SucceededLines,
    int FailedLines,
    // Falha do arquivo inteiro (ex.: não pôde ser lido).
    string? Error,
    string StorageUrl);
