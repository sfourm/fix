using Fix.Storage.Application.Abstractions.Authorization;
using Fix.Storage.Application.Abstractions.Context;
using Fix.Storage.Application.Abstractions.Messaging;
using Fix.Storage.Application.Files.Dtos;
using Fix.Storage.Domain.AggregateRoots.Files;

namespace Fix.Storage.Application.Files.Queries;

[RequireMembership]
public sealed record ListFileLinesQuery(Guid UserId, Guid OrganizationId, Guid FileId, FileLineStatus? Status, int Page, int PageSize)
    : IQuery<PagedList<FileLineDto>>, IOrganizationRequest;
