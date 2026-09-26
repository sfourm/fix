using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Domain.AggregateRoots.Roles;

namespace Fix.Application.Organizations.Commands;

/// <summary>Cria um grupo no organograma, abaixo do grupo pai (ou da raiz, se ParentGroupId for nulo).</summary>
[RequireRole(RoleCodes.CreateUser)]
public sealed record CreateGroupCommand(
    Guid UserId,
    Guid OrganizationId,
    string Name,
    IReadOnlyList<string> RuleCodes,
    Guid? ParentGroupId = null)
    : ICommand<Guid>, IOrganizationRequest;

