using Fix.Application.Abstractions.Messaging;

namespace Fix.Application.Auth.Commands;

public sealed record RegisterUserCommand(string Email, string Password, string FullName) : ICommand<Guid>;
