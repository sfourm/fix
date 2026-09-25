using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Rules.Dtos;

namespace Fix.Application.Rules.Queries;

/// <summary>Rules da organização: owner e user (sistema) e as alçadas personalizadas dela. Na FIX, os papéis internos.</summary>
[RequireMembership]
public sealed record ListRulesQuery(Guid UserId, Guid OrganizationId) : IQuery<IReadOnlyList<RuleDto>>, IOrganizationRequest;
