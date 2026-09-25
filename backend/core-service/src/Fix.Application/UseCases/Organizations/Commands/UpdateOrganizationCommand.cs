using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

[RequireRole(RoleCodes.EditOrganization)]
public sealed record UpdateOrganizationCommand(Guid UserId, Guid OrganizationId, string Name)
    : ICommand<OrganizationDto>, IOrganizationRequest;

