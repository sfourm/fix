using Fix.Application.Auth.Commands;
using Fix.Application.Auth.Dtos;
using Fix.Application.Auth.Queries;

namespace Fix.Application.Common.Interfaces.UseCases;

/// <summary>Autenticação e usuários (ASP.NET Core Identity). A entrada é validada na presentation (IValidationFactory) e autorizada pelo UseCaseGuard.</summary>
public interface IAuthService
{
    Task<Guid> RegisterUserAsync(RegisterUserCommand command, CancellationToken cancellationToken);

    Task<UserDto> AuthenticateUserAsync(AuthenticateUserCommand command, CancellationToken cancellationToken);

    Task<UserDto> GetUserAsync(GetUserQuery query, CancellationToken cancellationToken);
}
