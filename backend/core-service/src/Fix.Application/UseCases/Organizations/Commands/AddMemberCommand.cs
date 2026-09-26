using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Organizations;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>
/// Adiciona um membro como user (na organização FIX, como administrador interno). RuleCode opcional: código de uma
/// alçada personalizada da organização a atribuir já na entrada (vazio ou "user" = sem alçada).
/// </summary>
[RequireRole(RoleCodes.CreateUser)]
public sealed record AddMemberCommand(Guid UserId, Guid OrganizationId, string Email, string? RuleCode, Desk? Desk)
    : ICommand<Guid>, IOrganizationRequest;

