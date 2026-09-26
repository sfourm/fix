using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;

namespace Fix.Storage.Application.Files.Queries;

[RequireMembership]
public sealed record GetFileQuery(Guid UserId, Guid OrganizationId, Guid Id)
    : IQuery<FileDto>, IOrganizationRequest;
