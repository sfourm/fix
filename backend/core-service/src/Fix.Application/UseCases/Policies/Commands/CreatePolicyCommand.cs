using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Policies.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Policies.Commands;

[RequireRole(RoleCodes.CreatePolicy)]
public sealed record CreatePolicyCommand(
    Guid UserId,
    Guid OrganizationId,
    string Code,
    string Title,
    string Version,
    string? Description,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    bool UseTemplate)
    : ICommand<PolicyDto>, IOrganizationRequest;

