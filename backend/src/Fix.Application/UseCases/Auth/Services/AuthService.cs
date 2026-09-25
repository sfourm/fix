using Fix.Application.Abstractions.Authentication;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Auth.Commands;
using Fix.Application.Auth.Dtos;
using Fix.Application.Auth.Mappers;
using Fix.Application.Auth.Queries;

namespace Fix.Application.Auth.Services;

internal sealed class AuthService(IIdentityService identityService)
    : ICommandHandler<RegisterUserCommand, Guid>,
      ICommandHandler<AuthenticateUserCommand, UserDto>,
      IQueryHandler<GetUserQuery, UserDto>
{
    public Task<Guid> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken) =>
        identityService.CreateUserAsync(
            command.Email.Trim(),
            command.Password,
            command.FullName.Trim(),
            cancellationToken);

    public async Task<UserDto> HandleAsync(AuthenticateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.ValidateCredentialsAsync(command.Email.Trim(), command.Password, cancellationToken)
            ?? throw new UnauthenticatedException("E-mail ou senha inválidos.");

        return user.ToDto();
    }

    public async Task<UserDto> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(query.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário", query.UserId);

        return user.ToDto();
    }
}
