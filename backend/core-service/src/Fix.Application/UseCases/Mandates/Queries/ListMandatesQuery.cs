using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Mandates.Dtos;
using Fix.Domain.Abstractions;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Mandates.Queries;

[RequireRole(RoleCodes.ViewMandate)]
public sealed record ListMandatesQuery(
    Guid UserId,
    Guid OrganizationId,
    Guid? PolicyId,
    MandateStatus? Status,
    int Page,
    int PageSize)
    : IQuery<PagedList<MandateDto>>, IOrganizationRequest;

