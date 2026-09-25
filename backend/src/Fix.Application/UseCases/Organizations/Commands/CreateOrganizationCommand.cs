using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Organizations.Dtos;

namespace Fix.Application.Organizations.Commands;

/// <summary>Não exige tenant: o usuário se torna owner da nova organização.</summary>
public sealed record CreateOrganizationCommand(Guid UserId, string Name) : ICommand<OrganizationDto>, IUserRequest;
