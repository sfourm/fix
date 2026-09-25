using Fix.Application.Abstractions.Authorization;
using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;

namespace Fix.Application.Organizations.Commands;

/// <summary>Transfere a propriedade da organização para outro membro (só o owner atual ou a equipe interna FIX).</summary>
[RequireMembership]
public sealed record TransferOwnershipCommand(Guid UserId, Guid OrganizationId, Guid MemberId)
    : ICommand, IOrganizationRequest;
