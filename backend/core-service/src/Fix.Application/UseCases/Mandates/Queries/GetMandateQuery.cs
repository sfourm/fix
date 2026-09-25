using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Mandates.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Mandates.Queries;

[RequireRole(RoleCodes.ViewMandate)]
public sealed record GetMandateQuery(Guid UserId, Guid OrganizationId, Guid Id)
    : IQuery<MandateDto>, IOrganizationRequest;

