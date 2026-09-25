using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Mandates.Dtos;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Mandates.Commands;

[RequireRole(RoleCodes.CreateMandate)]
public sealed record IssueMandateCommand(
    Guid UserId,
    Guid OrganizationId,
    Guid PolicyId,
    Guid AxisId,
    MandateType Type,
    MandateTermsInput Terms)
    : ICommand<MandateDto>, IOrganizationRequest;

