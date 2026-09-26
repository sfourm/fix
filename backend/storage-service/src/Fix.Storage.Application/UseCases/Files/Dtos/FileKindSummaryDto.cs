using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Dtos;

public sealed record FileKindSummaryDto(
    FileKind Kind,
    int Total,
    int Processing,
    int WithErrors,
    DateTimeOffset? LastUploadedAt,
    // O usuário tem a regra exigida pelo tipo.
    bool CanUpload);
