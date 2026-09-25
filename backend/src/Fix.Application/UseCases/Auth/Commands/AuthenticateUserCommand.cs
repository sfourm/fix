using Fix.Application.Abstractions.Messaging;
using Fix.Application.Auth.Dtos;

namespace Fix.Application.Auth.Commands;

/// <summary>Valida as credenciais no Identity. A emissão de sessão/token para o front é do BFF.</summary>
public sealed record AuthenticateUserCommand(string Email, string Password) : ICommand<UserDto>;
