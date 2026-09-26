using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Substitui as alçadas atribuídas diretamente ao membro (a base owner/user não muda).</summary>
[RequireRole(RoleCodes.UpdateUser)]
public sealed record SetMemberAlcadasCommand(Guid UserId, Guid OrganizationId, Guid MemberId, IReadOnlyList<string> RuleCodes)
    : ICommand, IOrganizationRequest;
