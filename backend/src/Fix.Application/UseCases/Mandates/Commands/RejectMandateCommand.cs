using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Mandates.Dtos;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Mandates.Commands;

[RequireRole(RoleCodes.ApproveMandate)]
public sealed record RejectMandateCommand(Guid UserId, Guid OrganizationId, Guid Id, string Reason)
    : ICommand<MandateDto>, IOrganizationRequest;

