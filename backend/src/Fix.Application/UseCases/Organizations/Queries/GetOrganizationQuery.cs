using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;

namespace Fix.Application.Organizations.Queries;

[RequireMembership]
public sealed record GetOrganizationQuery(Guid UserId, Guid OrganizationId)
    : IQuery<OrganizationSetupDto>, IOrganizationRequest;
