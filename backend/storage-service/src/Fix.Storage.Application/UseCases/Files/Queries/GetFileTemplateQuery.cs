using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Queries;

[RequireMembership]
public sealed record GetFileTemplateQuery(Guid UserId, Guid OrganizationId, FileKind Kind)
    : IQuery<FileTemplateDto>, IOrganizationRequest;
