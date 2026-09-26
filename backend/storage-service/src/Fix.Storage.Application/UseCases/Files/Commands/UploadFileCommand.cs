using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Commands;

/// <summary>Envio de um arquivo: grava no S3, registra e publica o evento FileUpload.</summary>
[RequireMembership]
public sealed record UploadFileCommand(
    Guid UserId,
    Guid OrganizationId,
    FileKind Kind,
    string FileName,
    string ContentType,
    byte[] Content)
    : ICommand<FileDto>, IOrganizationRequest
{
    /// <summary>Limite por arquivo (o conteúdo trafega inteiro pelo gRPC).</summary>
    public const int MaxBytes = 20 * 1024 * 1024;

    /// <summary>Limite de linhas de dados de um arquivo processado.</summary>
    public const int MaxRows = 20_000;
}
