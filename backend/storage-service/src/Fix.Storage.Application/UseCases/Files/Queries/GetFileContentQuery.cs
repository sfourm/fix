using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;

namespace Fix.Storage.Application.Files.Queries;

/// <summary>Conteúdo do arquivo original, para o BFF repassar ao navegador.</summary>
[RequireMembership]
public sealed record GetFileContentQuery(Guid UserId, Guid OrganizationId, Guid Id)
    : IQuery<FileContentDto>, IOrganizationRequest;
