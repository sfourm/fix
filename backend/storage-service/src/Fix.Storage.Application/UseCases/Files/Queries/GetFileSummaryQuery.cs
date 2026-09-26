using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;

namespace Fix.Storage.Application.Files.Queries;

/// <summary>Um resumo por tipo que o usuário pode ver, com a permissão de envio (cards da tela de uploads).</summary>
[RequireMembership]
public sealed record GetFileSummaryQuery(Guid UserId, Guid OrganizationId)
    : IQuery<IReadOnlyList<FileKindSummaryDto>>, IOrganizationRequest;
