using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Mandates.Commands;
using Fix.Application.Mandates.Dtos;
using Fix.Domain.AggregateRoots.Mandates;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Mandates.Queries;

/// <summary>Enquadramento prévio (dentro/FORA) sem emitir o mandato — usado pelo formulário.</summary>
[RequireRole(RoleCodes.CreateMandate)]
public sealed record PreviewMandateComplianceQuery(
    Guid UserId,
    Guid OrganizationId,
    Guid PolicyId,
    Guid AxisId,
    MandateType Type,
    MandateTermsInput Terms)
    : IQuery<ComplianceDto>, IOrganizationRequest;

