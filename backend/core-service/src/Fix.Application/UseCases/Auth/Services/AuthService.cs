using Fix.Application.Abstractions.Authentication;
using Fix.Application.Abstractions.Exceptions;
using Fix.Application.Auth.Commands;
using Fix.Application.Auth.Dtos;
using Fix.Application.Auth.Mappers;
using Fix.Application.Auth.Queries;
using Fix.Application.Authorization;
using Fix.Application.Common.Interfaces.UseCases;

namespace Fix.Application.Auth.Services;

internal sealed class AuthService(IIdentityService identityService, InternalAccess internalAccess)
    : IAuthService
{
    public Task<Guid> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken) =>
        identityService.CreateUserAsync(
            command.Email.Trim(),
            command.Password,
            command.FullName.Trim(),
            cancellationToken);

    public async Task<UserDto> AuthenticateUserAsync(AuthenticateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.ValidateCredentialsAsync(command.Email.Trim(), command.Password, cancellationToken)
            ?? throw new UnauthenticatedException("E-mail ou senha inválidos.");

        return await ToDtoAsync(user, cancellationToken);
    }

    public async Task<UserDto> GetUserAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(query.UserId, cancellationToken)
            ?? throw new NotFoundException("Usuário", query.UserId);

        return await ToDtoAsync(user, cancellationToken);
    }

    /// <summary>Roles de plataforma do usuário: o papel na equipe interna FIX, se houver (nunca para usuários de clientes).</summary>
    private async Task<UserDto> ToDtoAsync(UserInfo user, CancellationToken cancellationToken)
    {
        var code = InternalAccess.CodeOf(await internalAccess.GetLevelAsync(user.Id, cancellationToken));
        return user.ToDto() with { Roles = code is null ? [] : [code] };
    }
}
