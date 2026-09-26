using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Queries;

/// <summary>Arquivos da organização, do mais recente ao mais antigo. Sem tipo: todos os que o usuário pode ver.</summary>
[RequireMembership]
public sealed record ListFilesQuery(Guid UserId, Guid OrganizationId, FileKind? Kind, int Page, int PageSize)
    : IQuery<PagedList<FileDto>>, IOrganizationRequest;
